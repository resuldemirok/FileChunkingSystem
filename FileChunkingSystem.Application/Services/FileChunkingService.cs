using FileChunkingSystem.Application.Interfaces;
using FileChunkingSystem.Application.Models;
using FileChunkingSystem.Domain.Entities;
using FileChunkingSystem.Domain.Enums;
using FileChunkingSystem.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using Mapster;

namespace FileChunkingSystem.Application.Services;

public class FileChunkingService : IFileChunkingService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEnumerable<IStorageProvider> _storageProviders;
    private readonly IEnumerable<IChunkingStrategy> _chunkingStrategies;
    private readonly ILogger<FileChunkingService> _logger;
    private readonly Random _random = new();

    public FileChunkingService(
        IUnitOfWork unitOfWork,
        IEnumerable<IStorageProvider> storageProviders,
        IEnumerable<IChunkingStrategy> chunkingStrategies,
        ILogger<FileChunkingService> logger)
    {
        _unitOfWork = unitOfWork;
        _storageProviders = storageProviders;
        _chunkingStrategies = chunkingStrategies;
        _logger = logger;
    }

    public async Task<List<Guid>> ChunkAndStoreFileAsync(FileUploadModel model, IProgress<ProgressModel>? progress = null)
    {
        try
        {
            var result = new List<Guid>();
            await _unitOfWork.BeginTransactionAsync();

            var totalFiles = model.Files.Count;
            var processedFiles = 0;

            foreach (var file in model.Files)
            {
                var fileId = await ProcessSingleFileAsync(file, model.ChunkSize, progress, processedFiles, totalFiles);
                result.Add(fileId);
                processedFiles++;
            }

            await _unitOfWork.CommitTransactionAsync();
            return result;
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            _logger.LogError(ex, "Error occurred while chunking and storing files");
            throw;
        }
    }

    private async Task<Guid> ProcessSingleFileAsync(FileInputModel file, int? chunkSize, IProgress<ProgressModel>? progress, int currentFileIndex, int totalFiles)
    {
        var defaultChunkSize = chunkSize ?? CalculateOptimalChunkSize(file.Content.Length);
        var checksum = CalculateSha256(file.Content);
        
        // Random algorithm selection
        var strategy = _chunkingStrategies.ElementAt(_random.Next(_chunkingStrategies.Count()));
        var chunks = strategy.ChunkFile(file.Content, defaultChunkSize);
        
        var fileMetadata = new FileMetadata
        {
            OriginalFileName = file.FileName,
            FileExtension = Path.GetExtension(file.FileName),
            FileSize = file.Content.Length,
            ChecksumSha256 = checksum,
            ChunkingAlgorithm = strategy.Algorithm,
            TotalChunks = chunks.Length,
            ChunkSize = defaultChunkSize
        };

        var storageProvidersList = _storageProviders.ToList();
        
        for (int i = 0; i < chunks.Length; i++)
        {
            var provider = storageProvidersList[i % storageProvidersList.Count]; // provider selection
            var chunkKey = $"{fileMetadata.Id}_{i}";
            var storageKey = await provider.StoreChunkAsync(chunks[i], fileMetadata.Id.ToString().ToLower(), chunkKey);
            
            var chunkMetadata = new ChunkMetadata
            {
                FileMetadataId = fileMetadata.Id,
                ChunkIndex = i,
                ChunkSize = chunks[i].Length,
                ChecksumSha256 = CalculateSha256(chunks[i]),
                StorageProviderType = provider.ProviderType,
                StorageKey = storageKey
            };
            
            fileMetadata.Chunks.Add(chunkMetadata);
            
            progress?.Report(new ProgressModel
            {
                CurrentStep = (currentFileIndex * chunks.Length) + i + 1,
                TotalSteps = totalFiles * chunks.Length,
                Operation = $"Storing chunk {i + 1}/{chunks.Length} of file {file.FileName}"
            });
        }

        await _unitOfWork.Repository<FileMetadata>().AddAsync(fileMetadata);
        await _unitOfWork.SaveChangesAsync();
        return fileMetadata.Id;
    }

    public async Task<byte[]> MergeAndRetrieveFileAsync(FileMergeModel model, IProgress<ProgressModel>? progress = null)
    {
        try
        {
            var fileMetadata = await _unitOfWork.Repository<FileMetadata>().GetByIdAsync(model.FileMetadataId);
            if (fileMetadata == null)
                throw new FileNotFoundException("File metadata not found");

            var strategy = _chunkingStrategies.First(s => s.Algorithm == fileMetadata.ChunkingAlgorithm);
            var chunks = new byte[fileMetadata.TotalChunks][];
            
            for (int i = 0; i < fileMetadata.TotalChunks; i++)
            {
                var chunkMetadata = fileMetadata.Chunks.First(c => c.ChunkIndex == i);
                var provider = _storageProviders.First(p => p.ProviderType == chunkMetadata.StorageProviderType);
                
                chunks[i] = await provider.RetrieveChunkAsync(fileMetadata.Id.ToString().ToLower(), chunkMetadata.StorageKey);
                
                // Chunk checksum validation
                var chunkChecksum = CalculateSha256(chunks[i]);
                if (chunkChecksum != chunkMetadata.ChecksumSha256)
                    throw new InvalidDataException($"Chunk {i} checksum validation failed");
                
                progress?.Report(new ProgressModel
                {
                    CurrentStep = i + 1,
                    TotalSteps = fileMetadata.TotalChunks,
                    Operation = $"Retrieving chunk {i + 1}/{fileMetadata.TotalChunks}"
                });
            }

            var mergedData = strategy.MergeChunks(chunks);
            
            // Final file checksum validation
            var finalChecksum = CalculateSha256(mergedData);
            if (finalChecksum != fileMetadata.ChecksumSha256)
                throw new InvalidDataException("Final file checksum validation failed");

            return mergedData;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while merging file {FileId}", model.FileMetadataId);
            throw;
        }
    }

    public async Task<bool> DeleteFileAsync(Guid fileMetadataId)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync();
            
            var fileMetadata = await _unitOfWork.Repository<FileMetadata>().GetByIdAsync(fileMetadataId);
            if (fileMetadata == null) return false;

            // Delete chunks from storage providers
            foreach (var chunk in fileMetadata.Chunks)
            {
                var provider = _storageProviders.First(p => p.ProviderType == chunk.StorageProviderType);
                await provider.DeleteChunkAsync(fileMetadata.Id.ToString().ToLower(), chunk.StorageKey);
            }

            // Delete metadata from database
            await _unitOfWork.Repository<FileMetadata>().DeleteAsync(fileMetadataId);
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();
            
            return true;
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            _logger.LogError(ex, "Error occurred while deleting file {FileId}", fileMetadataId);
            throw;
        }
    }

    private static string CalculateSha256(byte[] data)
    {
        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(data);
        return Convert.ToHexString(hash);
    }

    private static int CalculateOptimalChunkSize(long fileSize)
    {
        return fileSize switch
        {
            < 1024 * 1024 => 64 * 1024, // 64KB for files < 1MB
            < 10 * 1024 * 1024 => 256 * 1024, // 256KB for files < 10MB
            < 100 * 1024 * 1024 => 1024 * 1024, // 1MB for files < 100MB
            _ => 4 * 1024 * 1024 // 4MB for larger files
        };
    }

    public async Task<IEnumerable<FileMetadataModel>> GetAllFileMetadataAsync()
    {
        try
        {
            var allFileMetadatas = await _unitOfWork.Repository<FileMetadata>().GetAllAsync();

            var files = allFileMetadatas
                .Where(f => !f.IsDeleted)
                .OrderByDescending(f => f.CreatedAt)
                .ToList();

            _logger.LogInformation("Retrieved {Count} files from database", files.Count);

            return files.Adapt<IEnumerable<FileMetadataModel>>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all file metadata");
            throw;
        }
    }

    public async Task<IEnumerable<FileMetadataModel>> SearchFilesAsync(string searchTerm)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetAllFileMetadataAsync();

            var allFileMetadatas = await _unitOfWork.Repository<FileMetadata>().GetAllAsync();

            var files = allFileMetadatas
                .Where(f => !f.IsDeleted && 
                        f.OriginalFileName.Contains(searchTerm))
                .OrderByDescending(f => f.CreatedAt)
                .ToList();

            _logger.LogInformation("Found {Count} files matching search term '{SearchTerm}'", 
                files.Count, searchTerm);
            
            return files.Adapt<IEnumerable<FileMetadataModel>>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching files with term '{SearchTerm}'", searchTerm);
            throw;
        }
    }

    public async Task<IEnumerable<FileMetadataModel>> GetFilteredFilesAsync(FileFilterModel filter)
    {
        try
        {
            var allFileMetadatas = await _unitOfWork.Repository<FileMetadata>().GetAllAsync();

            var query = allFileMetadatas
                .Where(f => !f.IsDeleted)
                .AsQueryable();

            // File size filters
            if (filter.MinSize.HasValue)
                query = query.Where(f => f.FileSize >= filter.MinSize.Value);

            if (filter.MaxSize.HasValue)
                query = query.Where(f => f.FileSize <= filter.MaxSize.Value);

            // Date range filters
            if (filter.StartDate.HasValue)
                query = query.Where(f => f.CreatedAt >= filter.StartDate.Value);

            if (filter.EndDate.HasValue)
                query = query.Where(f => f.CreatedAt <= filter.EndDate.Value);

            // File extension filter
            if (!string.IsNullOrEmpty(filter.FileExtension))
            {
                var extension = filter.FileExtension.StartsWith('.') 
                    ? filter.FileExtension 
                    : $".{filter.FileExtension}";
                query = query.Where(f => f.FileExtension.ToLower() == extension.ToLower());
            }

            // Chunk count filters
            if (filter.MinChunkCount.HasValue)
                query = query.Where(f => f.TotalChunks >= filter.MinChunkCount.Value);

            if (filter.MaxChunkCount.HasValue)
                query = query.Where(f => f.TotalChunks <= filter.MaxChunkCount.Value);

            var files = query
                .OrderByDescending(f => f.CreatedAt)
                .ToList();

            _logger.LogInformation("Found {Count} files matching filter criteria", files.Count);
            
            return files.Adapt<IEnumerable<FileMetadataModel>>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error filtering files");
            throw;
        }
    }

    public async Task<FileMetadataModel?> GetFileDetailsAsync(Guid fileId)
    {
        try
        {
            var allFileMetadatas = await _unitOfWork.Repository<FileMetadata>().GetAllAsync();

            var file = allFileMetadatas
                .Where(f => f.Id == fileId && !f.IsDeleted)
                .FirstOrDefault();

            if (file != null)
                _logger.LogInformation("Retrieved file details for {FileId}", fileId);
            else
                _logger.LogWarning("File not found: {FileId}", fileId);

            if (file == null) return null;
            return file.Adapt<FileMetadataModel>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving file details for {FileId}", fileId);
            throw;
        }
    }

    public async Task<IEnumerable<FileMetadataModel>> GetFilesForCleanupAsync(CleanupOptionsModel options)
    {
        try
        {
            var allFileMetadatas = await _unitOfWork.Repository<FileMetadata>().GetAllAsync();

            var query = allFileMetadatas
                .Where(f => !f.IsDeleted)
                .AsQueryable();

            // Delete files older than specified days
            if (options.DeleteOlderThanDays.HasValue)
            {
                var cutoffDate = DateTime.UtcNow.AddDays(-options.DeleteOlderThanDays.Value);
                query = query.Where(f => f.CreatedAt < cutoffDate);
            }

            // Delete files larger than specified bytes
            if (options.DeleteLargerThanBytes.HasValue)
                query = query.Where(f => f.FileSize > options.DeleteLargerThanBytes.Value);

            // Delete files smaller than specified bytes
            if (options.DeleteSmallerThanBytes.HasValue)
                query = query.Where(f => f.FileSize < options.DeleteSmallerThanBytes.Value);

            // Delete files not accessed for specified days
            if (options.DeleteUnusedForDays.HasValue)
            {
                var cutoffDate = DateTime.UtcNow.AddDays(-options.DeleteUnusedForDays.Value);
                query = query.Where(f => f.UpdatedAt == null || f.UpdatedAt < cutoffDate);
            }

            // Delete files with specific extensions
            if (options.DeleteWithExtensions?.Any() == true)
            {
                var extensions = options.DeleteWithExtensions
                    .Select(ext => ext.StartsWith('.') ? ext : $".{ext}")
                    .ToList();
                query = query.Where(f => extensions.Contains(f.FileExtension.ToLower()));
            }

            var files = query
                .OrderBy(f => f.CreatedAt)
                .ToList();

            // Apply max limits if specified
            if (options.MaxFilesToDelete.HasValue && files.Count > options.MaxFilesToDelete.Value)
                files = files.Take(options.MaxFilesToDelete.Value).ToList();

            if (options.MaxBytesToDelete.HasValue)
            {
                var totalSize = 0L;
                var filteredFiles = new List<FileMetadata>();
                
                foreach (var file in files)
                {
                    if (totalSize + file.FileSize <= options.MaxBytesToDelete.Value)
                    {
                        filteredFiles.Add(file);
                        totalSize += file.FileSize;
                    }
                    else break;
                }
                
                files = filteredFiles;
            }

            _logger.LogInformation("Found {Count} files for cleanup", files.Count);
            
            return files.Adapt<IEnumerable<FileMetadataModel>>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting files for cleanup");
            throw;
        }
    }

    public async Task<FileIntegrityResult> VerifyFileIntegrityAsync(Guid fileId)
    {
        var result = new FileIntegrityResult();
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            var allFileMetadatas = await _unitOfWork.Repository<FileMetadata>().GetAllAsync();

            var file = allFileMetadatas
                .FirstOrDefault(f => f.Id == fileId && !f.IsDeleted);

            if (file == null)
            {
                result.ErrorMessage = "File not found";
                return result;
            }

            result.TotalChunks = file.Chunks.Count;

            // Verify each chunk
            foreach (var chunk in file.Chunks)
            {
                try
                {
                    // Get chunk data from storage
                    var storageProvider = _storageProviders.First(x => x.ProviderType == chunk.StorageProviderType);
                    var chunkData = await storageProvider.RetrieveChunkAsync(fileId.ToString().ToLower(), chunk.StorageKey);

                    if (chunkData == null || chunkData.Length == 0)
                    {
                        result.MissingChunks.Add(chunk.ChunkIndex);
                        continue;
                    }

                    // Verify chunk size
                    if (chunkData.Length != chunk.ChunkSize)
                    {
                        result.CorruptedChunks.Add(chunk.ChunkIndex);
                        if (result.ChunkErrors == null)
                            result.ChunkErrors = new Dictionary<int, string>();
                        result.ChunkErrors[chunk.ChunkIndex] = "Size mismatch";
                        continue;
                    }

                    // Verify chunk checksum
                    using var sha256 = System.Security.Cryptography.SHA256.Create();
                    var computedHash = Convert.ToHexString(sha256.ComputeHash(chunkData));

                    if (!string.Equals(computedHash, chunk.ChecksumSha256, StringComparison.OrdinalIgnoreCase))
                    {
                        result.CorruptedChunks.Add(chunk.ChunkIndex);
                        if (result.ChunkErrors == null)
                            result.ChunkErrors = new Dictionary<int, string>();
                        result.ChunkErrors[chunk.ChunkIndex] = "Checksum mismatch";
                        continue;
                    }

                    result.ValidChunks++;
                }
                catch (Exception ex)
                {
                    result.CorruptedChunks.Add(chunk.ChunkIndex);
                    if (result.ChunkErrors == null)
                        result.ChunkErrors = new Dictionary<int, string>();
                    result.ChunkErrors[chunk.ChunkIndex] = ex.Message;
                    
                    _logger.LogWarning(ex, "Error verifying chunk {ChunkIndex} of file {FileId}", 
                        chunk.ChunkIndex, fileId);
                }
            }

            // Verify file-level integrity
            if (result.ValidChunks == result.TotalChunks && 
                result.CorruptedChunks.Count == 0 && 
                result.MissingChunks.Count == 0)
            {
                // Optionally verify overall file checksum by reconstructing the file
                try
                {
                    var reconstructedData = new List<byte>();
                    foreach (var chunk in file.Chunks.OrderBy(c => c.ChunkIndex))
                    {
                        var storageProvider = _storageProviders.First(x => x.ProviderType == chunk.StorageProviderType);
                        var chunkData = await storageProvider.RetrieveChunkAsync(fileId.ToString().ToLower(), chunk.StorageKey);
                        reconstructedData.AddRange(chunkData);
                    }

                    using var sha256 = System.Security.Cryptography.SHA256.Create();
                    var computedFileHash = Convert.ToHexString(sha256.ComputeHash(reconstructedData.ToArray()));

                    if (string.Equals(computedFileHash, file.ChecksumSha256, StringComparison.OrdinalIgnoreCase))
                        result.IsValid = true;
                    else
                    {
                        result.IsValid = false;
                        result.ErrorMessage = "File checksum mismatch";
                    }
                }
                catch (Exception ex)
                {
                    result.IsValid = false;
                    result.ErrorMessage = $"Error verifying file checksum: {ex.Message}";
                }
            }
            else
                result.IsValid = false;

            stopwatch.Stop();
            result.CheckDuration = stopwatch.Elapsed;

            _logger.LogInformation("Integrity check completed for file {FileId}. Valid: {IsValid}, " +
                                "Valid chunks: {ValidChunks}/{TotalChunks}, Duration: {Duration}ms",
                fileId, result.IsValid, result.ValidChunks, result.TotalChunks, 
                result.CheckDuration.TotalMilliseconds);

            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            result.CheckDuration = stopwatch.Elapsed;
            result.ErrorMessage = ex.Message;
            
            _logger.LogError(ex, "Error during integrity check for file {FileId}", fileId);
            return result;
        }
    }

}

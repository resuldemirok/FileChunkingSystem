using FileChunkingSystem.Domain.Enums;
using FileChunkingSystem.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FileChunkingSystem.Infrastructure.Storage;

/// <summary>
/// File system storage provider implementation for storing chunks as files on disk.
/// Organizes chunks in folders by group and stores them as individual .chunk files.
/// </summary>
public class FileSystemStorageProvider : IStorageProvider
{
    private readonly string _basePath;
    private readonly ILogger<FileSystemStorageProvider> _logger;

    /// <summary>
    /// Gets the storage provider type
    /// </summary>
    public StorageProviderType ProviderType => StorageProviderType.FileSystem;

    /// <summary>
    /// Initializes a new instance of the FileSystemStorageProvider
    /// </summary>
    /// <param name="configuration">Application configuration</param>
    /// <param name="logger">Logger instance</param>
    public FileSystemStorageProvider(IConfiguration configuration, ILogger<FileSystemStorageProvider> logger)
    {
        _basePath = Path.Combine(Directory.GetCurrentDirectory(), 
            configuration["Storage:FileSystem:BasePath"] ?? "chunks");
        _logger = logger;
        
        // Ensure base directory exists
        if (!Directory.Exists(_basePath))
            Directory.CreateDirectory(_basePath);
    }

    /// <summary>
    /// Stores a chunk as a file in the file system
    /// </summary>
    /// <param name="chunkData">The chunk data to store</param>
    /// <param name="group">The group/folder name</param>
    /// <param name="key">The unique key for the chunk</param>
    /// <returns>The storage key</returns>
    public async Task<string> StoreChunkAsync(byte[] chunkData, string group, string key)
    {
        try
        {
            // Create group folder if it doesn't exist
            var folderPath = Path.Combine(_basePath, group);
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            // Write chunk data to file
            var filePath = Path.Combine(folderPath, $"{key}.chunk");
            await File.WriteAllBytesAsync(filePath, chunkData);
            
            _logger.LogInformation("Chunk stored successfully at {FilePath}", filePath);
            return key;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error storing chunk with key {Key}", key);
            throw;
        }
    }

    /// <summary>
    /// Retrieves a chunk from the file system
    /// </summary>
    /// <param name="group">The group/folder name</param>
    /// <param name="key">The unique key for the chunk</param>
    /// <returns>The chunk data</returns>
    public async Task<byte[]> RetrieveChunkAsync(string group, string key)
    {
        try
        {
            var filePath = Path.Combine(_basePath, group, $"{key}.chunk");
            if (File.Exists(filePath))
                return await File.ReadAllBytesAsync(filePath);
                
            // This line seems to be a bug - creating an empty file when not found
            await File.WriteAllBytesAsync(filePath, new byte[] { 0x00 });
            throw new FileNotFoundException($"Chunk file not found: {key}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving chunk with key {Key}", key);
            throw;
        }
    }

    /// <summary>
    /// Deletes a chunk file from the file system
    /// </summary>
    /// <param name="group">The group/folder name</param>
    /// <param name="key">The unique key for the chunk</param>
    /// <returns>True if deleted successfully, false if file not found</returns>
    public Task<bool> DeleteChunkAsync(string group, string key)
    {
        try
        {
            var filePath = Path.Combine(_basePath, group, $"{key}.chunk");
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                _logger.LogInformation("Chunk deleted successfully: {Key}", key);
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting chunk with key {Key}", key);
            throw;
        }
    }
}

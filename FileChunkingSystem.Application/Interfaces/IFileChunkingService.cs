using FileChunkingSystem.Application.Models;
using FileChunkingSystem.Domain.Entities;

namespace FileChunkingSystem.Application.Interfaces;

public interface IFileChunkingService
{
    Task<List<Guid>> ChunkAndStoreFileAsync(FileUploadModel model, IProgress<ProgressModel>? progress = null);
    Task<byte[]> MergeAndRetrieveFileAsync(FileMergeModel model, IProgress<ProgressModel>? progress = null);
    
    Task<IEnumerable<FileMetadataModel>> GetAllFileMetadataAsync();
    Task<IEnumerable<FileMetadataModel>> SearchFilesAsync(string searchTerm);
    Task<IEnumerable<FileMetadataModel>> GetFilteredFilesAsync(FileFilterModel filter);
    Task<FileMetadataModel?> GetFileDetailsAsync(Guid fileId);
    Task<IEnumerable<FileMetadataModel>> GetFilesForCleanupAsync(CleanupOptionsModel options);
    Task<FileIntegrityResult> VerifyFileIntegrityAsync(Guid fileId);
    
    Task<bool> DeleteFileAsync(Guid fileMetadataId);
    
}

using FileChunkingSystem.Application.Models;
using FileChunkingSystem.Domain.Entities;

namespace FileChunkingSystem.Application.Interfaces;

/// <summary>
/// Service interface for file chunking and merging operations
/// </summary>
public interface IFileChunkingService
{
    /// <summary>
    /// Chunks and stores files from the upload model
    /// </summary>
    /// <param name="model">The file upload model containing files to process</param>
    /// <param name="progress">Optional progress reporting callback</param>
    /// <returns>A list of file metadata identifiers for the processed files</returns>
    Task<List<Guid>> ChunkAndStoreFileAsync(FileUploadModel model, IProgress<ProgressModel>? progress = null);
    
    /// <summary>
    /// Merges file chunks and retrieves the complete file data
    /// </summary>
    /// <param name="model">The file merge model specifying which file to merge</param>
    /// <param name="progress">Optional progress reporting callback</param>
    /// <returns>The complete file data as a byte array</returns>
    Task<byte[]> MergeAndRetrieveFileAsync(FileMergeModel model, IProgress<ProgressModel>? progress = null);
    
    /// <summary>
    /// Retrieves metadata for all stored files
    /// </summary>
    /// <returns>A collection of file metadata models</returns>
    Task<IEnumerable<FileMetadataModel>> GetAllFileMetadataAsync();
    
    /// <summary>
    /// Searches for files based on a search term
    /// </summary>
    /// <param name="searchTerm">The term to search for in file names</param>
    /// <returns>A collection of matching file metadata models</returns>
    Task<IEnumerable<FileMetadataModel>> SearchFilesAsync(string searchTerm);
    
    /// <summary>
    /// Retrieves files that match the specified filter criteria
    /// </summary>
    /// <param name="filter">The filter criteria to apply</param>
    /// <returns>A collection of filtered file metadata models</returns>
    Task<IEnumerable<FileMetadataModel>> GetFilteredFilesAsync(FileFilterModel filter);
    
    /// <summary>
    /// Retrieves detailed information about a specific file
    /// </summary>
    /// <param name="fileId">The unique identifier of the file</param>
    /// <returns>The file metadata model if found, otherwise null</returns>
    Task<FileMetadataModel?> GetFileDetailsAsync(Guid fileId);
    
    /// <summary>
    /// Retrieves files that match cleanup criteria for potential deletion
    /// </summary>
    /// <param name="options">The cleanup options specifying criteria</param>
    /// <returns>A collection of file metadata models eligible for cleanup</returns>
    Task<IEnumerable<FileMetadataModel>> GetFilesForCleanupAsync(CleanupOptionsModel options);
    
    /// <summary>
    /// Verifies the integrity of a file by checking all its chunks
    /// </summary>
    /// <param name="fileId">The unique identifier of the file to verify</param>
    /// <returns>The integrity verification result</returns>
    Task<FileIntegrityResult> VerifyFileIntegrityAsync(Guid fileId);
    
    /// <summary>
    /// Deletes a file and all its associated chunks
    /// </summary>
    /// <param name="fileMetadataId">The unique identifier of the file to delete</param>
    /// <returns>True if the file was successfully deleted, otherwise false</returns>
    Task<bool> DeleteFileAsync(Guid fileMetadataId);
}

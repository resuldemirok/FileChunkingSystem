using FileChunkingSystem.Domain.Enums;

namespace FileChunkingSystem.Domain.Interfaces;

/// <summary>
/// Interface for storage providers that handle chunk storage operations
/// </summary>
public interface IStorageProvider
{
    /// <summary>
    /// Gets the type of storage provider
    /// </summary>
    StorageProviderType ProviderType { get; }
    
    /// <summary>
    /// Stores a chunk of data in the storage system
    /// </summary>
    /// <param name="chunkData">The chunk data to store</param>
    /// <param name="group">The group identifier for organizing chunks</param>
    /// <param name="key">The unique key for the chunk</param>
    /// <returns>The storage key where the chunk was stored</returns>
    Task<string> StoreChunkAsync(byte[] chunkData, string group, string key);
    
    /// <summary>
    /// Retrieves a chunk of data from the storage system
    /// </summary>
    /// <param name="group">The group identifier</param>
    /// <param name="key">The unique key of the chunk</param>
    /// <returns>The chunk data</returns>
    Task<byte[]> RetrieveChunkAsync(string group, string key);
    
    /// <summary>
    /// Deletes a chunk from the storage system
    /// </summary>
    /// <param name="group">The group identifier</param>
    /// <param name="key">The unique key of the chunk to delete</param>
    /// <returns>True if the chunk was successfully deleted, otherwise false</returns>
    Task<bool> DeleteChunkAsync(string group, string key);
}

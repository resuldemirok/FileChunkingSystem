using FileChunkingSystem.Domain.Entities;
using FileChunkingSystem.Domain.Enums;
using FileChunkingSystem.Domain.Interfaces;
using FileChunkingSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FileChunkingSystem.Infrastructure.Storage;

/// <summary>
/// PostgreSQL storage provider implementation for storing chunks in PostgreSQL database.
/// Uses Entity Framework Core to manage chunk storage in the storage database.
/// </summary>
public class PostgreSQLStorageProvider : IStorageProvider
{
    private readonly StorageDbContext _storageContext;
    private readonly ILogger<PostgreSQLStorageProvider> _logger;

    /// <summary>
    /// Gets the storage provider type
    /// </summary>
    public StorageProviderType ProviderType => StorageProviderType.PostgreSQL;

    /// <summary>
    /// Initializes a new instance of the PostgreSQLStorageProvider
    /// </summary>
    /// <param name="storageContext">The storage database context</param>
    /// <param name="logger">Logger instance</param>
    public PostgreSQLStorageProvider(
        StorageDbContext storageContext, 
        ILogger<PostgreSQLStorageProvider> logger)
    {
        _storageContext = storageContext;
        _logger = logger;
    }

    /// <summary>
    /// Stores a chunk in PostgreSQL database
    /// </summary>
    /// <param name="chunkData">The chunk data to store</param>
    /// <param name="group">The group name</param>
    /// <param name="key">The unique key for the chunk</param>
    /// <returns>The storage key</returns>
    public async Task<string> StoreChunkAsync(byte[] chunkData, string group, string key)
    {
        try
        {
            // Check if chunk already exists
            var existingChunk = await _storageContext.ChunkStorage
                .FirstOrDefaultAsync(c => c.Key == key);

            if (existingChunk != null)
            {
                // Update existing chunk
                existingChunk.Data = chunkData;
                existingChunk.GroupName = group;
                existingChunk.CreatedAt = DateTime.UtcNow;
                
                _storageContext.ChunkStorage.Update(existingChunk);
            }
            else
            {
                // Create new chunk
                var chunkStorage = new ChunkStorage
                {
                    Key = key,
                    GroupName = group,
                    Data = chunkData,
                    CreatedAt = DateTime.UtcNow
                };

                await _storageContext.ChunkStorage.AddAsync(chunkStorage);
            }

            await _storageContext.SaveChangesAsync();
            _logger.LogInformation("Chunk stored successfully in PostgreSQL with key {Key}", key);
            return key;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error storing chunk in PostgreSQL with key {Key}", key);
            throw;
        }
    }

    /// <summary>
    /// Retrieves a chunk from PostgreSQL database
    /// </summary>
    /// <param name="group">The group name</param>
    /// <param name="key">The unique key for the chunk</param>
    /// <returns>The chunk data</returns>
    public async Task<byte[]> RetrieveChunkAsync(string group, string key)
    {
        try
        {
            var chunk = await _storageContext.ChunkStorage
                .FirstOrDefaultAsync(c => c.Key == key && c.GroupName == group);

            if (chunk != null) return chunk.Data;
            
            throw new KeyNotFoundException($"Chunk not found with key: {key} in group: {group}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving chunk from PostgreSQL with key {Key}", key);
            throw;
        }
    }

    /// <summary>
    /// Deletes a chunk from PostgreSQL database
    /// </summary>
    /// <param name="group">The group name</param>
    /// <param name="key">The unique key for the chunk</param>
    /// <returns>True if deleted successfully, false if not found</returns>
    public async Task<bool> DeleteChunkAsync(string group, string key)
    {
        try
        {
            var chunk = await _storageContext.ChunkStorage
                .FirstOrDefaultAsync(c => c.Key == key && c.GroupName == group);

            if (chunk != null)
            {
                _storageContext.ChunkStorage.Remove(chunk);
                await _storageContext.SaveChangesAsync();
                
                _logger.LogInformation("Chunk deleted from PostgreSQL: {Key}", key);
                return true;
            }
            
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting chunk from PostgreSQL with key {Key}", key);
            throw;
        }
    }
}

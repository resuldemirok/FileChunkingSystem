using FileChunkingSystem.Domain.Entities;
using FileChunkingSystem.Domain.Enums;
using FileChunkingSystem.Domain.Interfaces;
using FileChunkingSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FileChunkingSystem.Infrastructure.Storage;

public class PostgreSQLStorageProvider : IStorageProvider
{
    private readonly StorageDbContext _storageContext;
    private readonly ILogger<PostgreSQLStorageProvider> _logger;

    public StorageProviderType ProviderType => StorageProviderType.PostgreSQL;

    public PostgreSQLStorageProvider(
        StorageDbContext storageContext, 
        ILogger<PostgreSQLStorageProvider> logger)
    {
        _storageContext = storageContext;
        _logger = logger;
    }

    public async Task<string> StoreChunkAsync(byte[] chunkData, string group, string key)
    {
        try
        {
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
            
            _logger.LogInformation("Chunk stored successfully with key {Key}", key);
            return key;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error storing chunk with key {Key}", key);
            throw;
        }
    }

    public async Task<byte[]> RetrieveChunkAsync(string group, string key)
    {
        try
        {
            var chunkData = await _storageContext.ChunkStorage
                .Where(c => c.GroupName == group && c.Key == key)
                .Select(c => c.Data)
                .FirstOrDefaultAsync();

            if (chunkData == null)
                throw new KeyNotFoundException($"Chunk not found with key: {key}");

            _logger.LogDebug("Chunk retrieved successfully with key {Key}", key);
            return chunkData;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving chunk with key {Key}", key);
            throw;
        }
    }

    public async Task<bool> DeleteChunkAsync(string group, string key)
    {
        try
        {
            var chunkStorage = await _storageContext.ChunkStorage
                .FirstOrDefaultAsync(c => c.GroupName == group && c.Key == key);

            if (chunkStorage == null)
            {
                _logger.LogWarning("Chunk not found for deletion with key {Key}", key);
                return false;
            }

            _storageContext.ChunkStorage.Remove(chunkStorage);
            var result = await _storageContext.SaveChangesAsync();

            _logger.LogInformation("Chunk deleted successfully with key {Key}", key);
            return result > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting chunk with key {Key}", key);
            throw;
        }
    }
}

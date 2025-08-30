using FileChunkingSystem.Domain.Enums;
using FileChunkingSystem.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FileChunkingSystem.Infrastructure.Storage;

public class FileSystemStorageProvider : IStorageProvider
{
    private readonly string _basePath;
    private readonly ILogger<FileSystemStorageProvider> _logger;

    public StorageProviderType ProviderType => StorageProviderType.FileSystem;

    public FileSystemStorageProvider(IConfiguration configuration, ILogger<FileSystemStorageProvider> logger)
    {
        _basePath = Path.Combine(Directory.GetCurrentDirectory(), configuration["Storage:FileSystem:BasePath"] ?? "chunks");
        _logger = logger;
        
        if (!Directory.Exists(_basePath))
            Directory.CreateDirectory(_basePath);
    }

    public async Task<string> StoreChunkAsync(byte[] chunkData, string group, string key)
    {
        try
        {
            var folderPath = Path.Combine(_basePath, group);
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

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

    public async Task<byte[]> RetrieveChunkAsync(string group, string key)
    {
        try
        {
            var filePath = Path.Combine(_basePath, group, $"{key}.chunk");
            if (File.Exists(filePath))
                return await File.ReadAllBytesAsync(filePath);
            await File.WriteAllBytesAsync(filePath, new byte[] { 0x00 });
            throw new FileNotFoundException($"Chunk file not found: {key}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving chunk with key {Key}", key);
            throw;
        }
    }

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

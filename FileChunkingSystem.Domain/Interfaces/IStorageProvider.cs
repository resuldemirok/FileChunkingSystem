using FileChunkingSystem.Domain.Enums;

namespace FileChunkingSystem.Domain.Interfaces;

public interface IStorageProvider
{
    StorageProviderType ProviderType { get; }
    Task<string> StoreChunkAsync(byte[] chunkData, string group, string key);
    Task<byte[]> RetrieveChunkAsync(string group, string key);
    Task<bool> DeleteChunkAsync(string group, string key);
}

using FileChunkingSystem.Domain.Enums;

namespace FileChunkingSystem.Domain.Entities;

public class ChunkMetadata : BaseEntity
{
    public Guid FileMetadataId { get; set; }
    public virtual FileMetadata FileMetadata { get; set; } = null!;
    public int ChunkIndex { get; set; }
    public int ChunkSize { get; set; }
    public string ChecksumSha256 { get; set; } = string.Empty;
    public StorageProviderType StorageProviderType { get; set; }
    public string StorageKey { get; set; } = string.Empty;
    public byte[] ChunkData { get; set; } = Array.Empty<byte>();
}

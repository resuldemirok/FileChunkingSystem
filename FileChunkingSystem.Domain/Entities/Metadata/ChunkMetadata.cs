using FileChunkingSystem.Domain.Enums;

namespace FileChunkingSystem.Domain.Entities;

/// <summary>
/// Entity representing metadata for a file chunk
/// </summary>
public class ChunkMetadata : BaseEntity
{
    /// <summary>
    /// Gets or sets the identifier of the parent file metadata
    /// </summary>
    public Guid FileMetadataId { get; set; }
    
    /// <summary>
    /// Gets or sets the navigation property to the parent file metadata
    /// </summary>
    public virtual FileMetadata FileMetadata { get; set; } = null!;
    
    /// <summary>
    /// Gets or sets the index of this chunk within the file
    /// </summary>
    public int ChunkIndex { get; set; }
    
    /// <summary>
    /// Gets or sets the size of this chunk in bytes
    /// </summary>
    public int ChunkSize { get; set; }
    
    /// <summary>
    /// Gets or sets the SHA256 checksum of this chunk
    /// </summary>
    public string ChecksumSha256 { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the storage provider type used for this chunk
    /// </summary>
    public StorageProviderType StorageProviderType { get; set; }
    
    /// <summary>
    /// Gets or sets the storage key for retrieving this chunk
    /// </summary>
    public string StorageKey { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the actual chunk data
    /// </summary>
    public byte[] ChunkData { get; set; } = Array.Empty<byte>();
}

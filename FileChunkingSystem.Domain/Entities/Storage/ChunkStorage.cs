using System.ComponentModel.DataAnnotations;

namespace FileChunkingSystem.Domain.Entities;

/// <summary>
/// Entity for storing chunk data in the database
/// </summary>
public class ChunkStorage : BaseEntity
{
    /// <summary>
    /// Gets or sets the unique storage key for the chunk
    /// </summary>
    [Key]
    public string Key { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the group name for organizing related chunks
    /// </summary>
    public string? GroupName { get; set; }
    
    /// <summary>
    /// Gets or sets the chunk data
    /// </summary>
    public byte[] Data { get; set; } = Array.Empty<byte>();
}

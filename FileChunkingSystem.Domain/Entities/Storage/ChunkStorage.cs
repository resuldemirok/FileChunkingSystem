using System.ComponentModel.DataAnnotations;

namespace FileChunkingSystem.Domain.Entities;

public class ChunkStorage : BaseEntity
{
    [Key]
    public string Key { get; set; } = string.Empty;
    
    public string? GroupName { get; set; }
    
    public byte[] Data { get; set; } = Array.Empty<byte>();
}

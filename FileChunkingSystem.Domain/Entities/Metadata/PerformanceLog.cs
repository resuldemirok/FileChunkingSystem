namespace FileChunkingSystem.Domain.Entities;

public class PerformanceLog : BaseEntity
{
    public Guid? FileMetadataId { get; set; }
    public string OperationType { get; set; } = string.Empty; // "Chunking", "Merging", "Storage", etc.
    public DateTime Timestamp { get; set; }
    public double DurationMs { get; set; }
    public long BytesProcessed { get; set; }
    public int ChunksProcessed { get; set; }
    public string? ErrorMessage { get; set; }
    public string? AdditionalData { get; set; }
    
    public virtual FileMetadata? FileMetadata { get; set; }
}

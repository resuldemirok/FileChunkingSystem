namespace FileChunkingSystem.Domain.Entities;

/// <summary>
/// Entity for logging performance metrics and operation statistics
/// </summary>
public class PerformanceLog : BaseEntity
{
    /// <summary>
    /// Gets or sets the identifier of the related file metadata (optional)
    /// </summary>
    public Guid? FileMetadataId { get; set; }
    
    /// <summary>
    /// Gets or sets the type of operation being logged
    /// </summary>
    public string OperationType { get; set; } = string.Empty; // "Chunking", "Merging", "Storage", etc.
    
    /// <summary>
    /// Gets or sets the timestamp when the operation occurred
    /// </summary>
    public DateTime Timestamp { get; set; }
    
    /// <summary>
    /// Gets or sets the duration of the operation in milliseconds
    /// </summary>
    public double DurationMs { get; set; }
    
    /// <summary>
    /// Gets or sets the number of bytes processed during the operation
    /// </summary>
    public long BytesProcessed { get; set; }
    
    /// <summary>
    /// Gets or sets the number of chunks processed during the operation
    /// </summary>
    public int ChunksProcessed { get; set; }
    
    /// <summary>
    /// Gets or sets the error message if the operation failed
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Gets or sets additional data related to the operation
    /// </summary>
    public string? AdditionalData { get; set; }
    
    /// <summary>
    /// Gets or sets the navigation property to the related file metadata
    /// </summary>
    public virtual FileMetadata? FileMetadata { get; set; }
}

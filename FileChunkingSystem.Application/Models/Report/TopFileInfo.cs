namespace FileChunkingSystem.Application.Models;

/// <summary>
/// Model representing information about top performing or problematic files
/// </summary>
public class TopFileInfo : BaseModel
{
    /// <summary>
    /// Gets or sets the unique identifier of the file
    /// </summary>
    public Guid FileId { get; set; }
    
    /// <summary>
    /// Gets or sets the name of the file
    /// </summary>
    public string FileName { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the size of the file in bytes
    /// </summary>
    public long FileSize { get; set; }
    
    /// <summary>
    /// Gets or sets the number of chunks in the file
    /// </summary>
    public int ChunkCount { get; set; }
    
    /// <summary>
    /// Gets or sets the chunking strategy used for the file
    /// </summary>
    public string? ChunkingStrategy { get; set; }
    
    /// <summary>
    /// Gets or sets the timestamp when the file was processed
    /// </summary>
    public DateTime ProcessedAt { get; set; }
    
    /// <summary>
    /// Gets or sets the time taken to process the file
    /// </summary>
    public TimeSpan ProcessingTime { get; set; }
    
    /// <summary>
    /// Gets or sets the MIME type of the file
    /// </summary>
    public string? MimeType { get; set; }
    
    /// <summary>
    /// Gets or sets the current status of the file
    /// </summary>
    public string Status { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the total number of times the file has been downloaded
    /// </summary>
    public long TotalDownloads { get; set; }
    
    /// <summary>
    /// Gets or sets the timestamp when the file was last accessed
    /// </summary>
    public DateTime? LastAccessedAt { get; set; }
    
    /// <summary>
    /// Gets the average chunk size for this file
    /// </summary>
    public double AverageChunkSize => ChunkCount > 0 ? (double)FileSize / ChunkCount : 0;
    
    /// <summary>
    /// Gets or sets the storage type used for the file
    /// </summary>
    public string? StorageType { get; set; }
    
    /// <summary>
    /// Gets or sets a value indicating whether the file has any errors
    /// </summary>
    public bool HasErrors { get; set; }
    
    /// <summary>
    /// Gets or sets the total number of errors associated with the file
    /// </summary>
    public int ErrorCount { get; set; }
    
    /// <summary>
    /// Gets or sets additional metrics specific to this file
    /// </summary>
    public Dictionary<string, object>? AdditionalMetrics { get; set; }
}

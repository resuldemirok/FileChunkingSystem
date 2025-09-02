namespace FileChunkingSystem.Application.Models;

/// <summary>
/// Model representing detailed error information
/// </summary>
public class ErrorDetail : BaseModel
{
    /// <summary>
    /// Gets or sets the identifier of the file associated with the error
    /// </summary>
    public Guid? FileId { get; set; }
    
    /// <summary>
    /// Gets or sets the name of the file associated with the error
    /// </summary>
    public string? FileName { get; set; }
    
    /// <summary>
    /// Gets or sets the type of error that occurred
    /// </summary>
    public string ErrorType { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the error message
    /// </summary>
    public string ErrorMessage { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the stack trace of the error
    /// </summary>
    public string? StackTrace { get; set; }
    
    /// <summary>
    /// Gets or sets the operation during which the error occurred
    /// </summary>
    public string Operation { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the timestamp when the error occurred
    /// </summary>
    public DateTime OccurredAt { get; set; }
    
    /// <summary>
    /// Gets or sets the severity level of the error (Low, Medium, High, Critical)
    /// </summary>
    public string Severity { get; set; } = "Medium";
    
    /// <summary>
    /// Gets or sets a value indicating whether the error has been resolved
    /// </summary>
    public bool IsResolved { get; set; }
    
    /// <summary>
    /// Gets or sets the timestamp when the error was resolved
    /// </summary>
    public DateTime? ResolvedAt { get; set; }
    
    /// <summary>
    /// Gets or sets the resolution description
    /// </summary>
    public string? Resolution { get; set; }
    
    /// <summary>
    /// Gets or sets additional context information about the error
    /// </summary>
    public Dictionary<string, object>? Context { get; set; }
    
    /// <summary>
    /// Gets or sets the chunk index if the error is chunk-specific
    /// </summary>
    public int? ChunkIndex { get; set; }
    
    /// <summary>
    /// Gets or sets the storage type where the error occurred
    /// </summary>
    public string? StorageType { get; set; }
    
    /// <summary>
    /// Gets or sets the file size associated with the error
    /// </summary>
    public long? FileSize { get; set; }
    
    /// <summary>
    /// Gets or sets the user agent string if applicable
    /// </summary>
    public string? UserAgent { get; set; }
    
    /// <summary>
    /// Gets or sets the IP address associated with the error
    /// </summary>
    public string? IpAddress { get; set; }
    
    /// <summary>
    /// Gets or sets the number of retry attempts made
    /// </summary>
    public int RetryCount { get; set; }
    
    /// <summary>
    /// Gets or sets a value indicating whether the operation can be retried
    /// </summary>
    public bool CanRetry { get; set; } = true;
}

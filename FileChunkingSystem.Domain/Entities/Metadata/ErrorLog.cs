namespace FileChunkingSystem.Domain.Entities;

/// <summary>
/// Entity for logging system errors and exceptions
/// </summary>
public class ErrorLog : BaseEntity
{
    /// <summary>
    /// Gets or sets the error message
    /// </summary>
    public string Message { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the stack trace of the error
    /// </summary>
    public string StackTrace { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the source of the error
    /// </summary>
    public string Source { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the timestamp when the error occurred
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

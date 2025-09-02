namespace FileChunkingSystem.Application.Models;

/// <summary>
/// Model representing file integrity verification results
/// </summary>
public class FileIntegrityResult : BaseModel
{
    /// <summary>
    /// Gets or sets a value indicating whether the file is valid
    /// </summary>
    public bool IsValid { get; set; }
    
    /// <summary>
    /// Gets or sets the total number of chunks in the file
    /// </summary>
    public int TotalChunks { get; set; }
    
    /// <summary>
    /// Gets or sets the number of valid chunks
    /// </summary>
    public int ValidChunks { get; set; }
    
    /// <summary>
    /// Gets or sets the list of corrupted chunk indices
    /// </summary>
    public List<int> CorruptedChunks { get; set; } = new();
    
    /// <summary>
    /// Gets or sets the list of missing chunk indices
    /// </summary>
    public List<int> MissingChunks { get; set; } = new();
    
    /// <summary>
    /// Gets or sets the error message if validation failed
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Gets or sets the timestamp when the integrity check was performed
    /// </summary>
    public DateTime CheckedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Gets or sets the duration of the integrity check
    /// </summary>
    public TimeSpan CheckDuration { get; set; }
    
    /// <summary>
    /// Gets or sets specific error messages for each corrupted chunk
    /// </summary>
    public Dictionary<int, string>? ChunkErrors { get; set; }
}

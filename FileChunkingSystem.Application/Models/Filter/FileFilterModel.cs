namespace FileChunkingSystem.Application.Models;

/// <summary>
/// Model representing filter criteria for file searches
/// </summary>
public class FileFilterModel : BaseModel
{
    /// <summary>
    /// Gets or sets the minimum file size filter in bytes
    /// </summary>
    public long? MinSize { get; set; }
    
    /// <summary>
    /// Gets or sets the maximum file size filter in bytes
    /// </summary>
    public long? MaxSize { get; set; }
    
    /// <summary>
    /// Gets or sets the start date filter for file creation
    /// </summary>
    public DateTime? StartDate { get; set; }
    
    /// <summary>
    /// Gets or sets the end date filter for file creation
    /// </summary>
    public DateTime? EndDate { get; set; }
    
    /// <summary>
    /// Gets or sets the file extension filter
    /// </summary>
    public string? FileExtension { get; set; }
    
    /// <summary>
    /// Gets or sets the minimum chunk count filter
    /// </summary>
    public int? MinChunkCount { get; set; }
    
    /// <summary>
    /// Gets or sets the maximum chunk count filter
    /// </summary>
    public int? MaxChunkCount { get; set; }
    
    /// <summary>
    /// Gets or sets the file status filter
    /// </summary>
    public string? Status { get; set; }
    
    /// <summary>
    /// Gets or sets the MIME type filter
    /// </summary>
    public string? MimeType { get; set; }
    
    /// <summary>
    /// Gets or sets a value indicating whether to filter files with errors
    /// </summary>
    public bool? HasErrors { get; set; }
    
    /// <summary>
    /// Gets or sets the filter for files last accessed before this date
    /// </summary>
    public DateTime? LastAccessedBefore { get; set; }
    
    /// <summary>
    /// Gets or sets the filter for files last accessed after this date
    /// </summary>
    public DateTime? LastAccessedAfter { get; set; }
    
    /// <summary>
    /// Gets or sets the list of tags to filter by
    /// </summary>
    public List<string>? Tags { get; set; }
    
    /// <summary>
    /// Gets or sets custom metadata filters as key-value pairs
    /// </summary>
    public Dictionary<string, string>? CustomMetadata { get; set; }
}

namespace FileChunkingSystem.Application.Models;

/// <summary>
/// Model representing comprehensive error statistics
/// </summary>
public class ErrorStatistics : BaseModel
{
    /// <summary>
    /// Gets or sets the total number of errors
    /// </summary>
    public int TotalErrors { get; set; }
    
    /// <summary>
    /// Gets or sets the breakdown of errors by type
    /// </summary>
    public Dictionary<string, int> ErrorsByType { get; set; } = new();
    
    /// <summary>
    /// Gets or sets the breakdown of errors by operation
    /// </summary>
    public Dictionary<string, int> ErrorsByOperation { get; set; } = new();
    
    /// <summary>
    /// Gets or sets the breakdown of errors by date
    /// </summary>
    public Dictionary<DateTime, int> ErrorsByDate { get; set; } = new();
    
    /// <summary>
    /// Gets or sets the breakdown of errors by file type
    /// </summary>
    public Dictionary<string, int> ErrorsByFileType { get; set; } = new();
    
    /// <summary>
    /// Gets or sets the breakdown of errors by storage type
    /// </summary>
    public Dictionary<string, int> ErrorsByStorageType { get; set; } = new();
    
    /// <summary>
    /// Gets or sets the list of recent errors
    /// </summary>
    public List<ErrorDetail> RecentErrors { get; set; } = new();
    
    /// <summary>
    /// Gets or sets the list of critical errors
    /// </summary>
    public List<ErrorDetail> CriticalErrors { get; set; } = new();
    
    /// <summary>
    /// Gets or sets the overall error rate as a percentage
    /// </summary>
    public double ErrorRate { get; set; }
    
    /// <summary>
    /// Gets or sets the timestamp of the first recorded error
    /// </summary>
    public DateTime? FirstErrorAt { get; set; }
    
    /// <summary>
    /// Gets or sets the timestamp of the last recorded error
    /// </summary>
    public DateTime? LastErrorAt { get; set; }
    
    /// <summary>
    /// Gets or sets the average time between errors
    /// </summary>
    public TimeSpan? AverageTimeBetweenErrors { get; set; }
    
    /// <summary>
    /// Gets or sets the error trends by type
    /// </summary>
    public Dictionary<string, ErrorTrend> ErrorTrends { get; set; } = new();
}

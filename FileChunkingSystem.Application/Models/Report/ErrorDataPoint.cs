namespace FileChunkingSystem.Application.Models;

/// <summary>
/// Model representing error data points for trend analysis
/// </summary>
public class ErrorDataPoint : BaseModel
{
    /// <summary>
    /// Gets or sets the date of the error data point
    /// </summary>
    public DateTime Date { get; set; }
    
    /// <summary>
    /// Gets or sets the total number of errors on this date
    /// </summary>
    public int ErrorCount { get; set; }
    
    /// <summary>
    /// Gets or sets the error rate as a percentage
    /// </summary>
    public double ErrorRate { get; set; }
    
    /// <summary>
    /// Gets or sets the breakdown of errors by type
    /// </summary>
    public Dictionary<string, int>? ErrorBreakdown { get; set; }
}

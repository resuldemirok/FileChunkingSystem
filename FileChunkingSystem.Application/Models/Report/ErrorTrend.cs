namespace FileChunkingSystem.Application.Models;

/// <summary>
/// Model representing error trend analysis data
/// </summary>
public class ErrorTrend : BaseModel
{
    /// <summary>
    /// Gets or sets the error type for this trend
    /// </summary>
    public string ErrorType { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the list of data points for trend analysis
    /// </summary>
    public List<ErrorDataPoint> DataPoints { get; set; } = new();
    
    /// <summary>
    /// Gets or sets the trend direction (positive = increasing, negative = decreasing)
    /// </summary>
    public double TrendDirection { get; set; }
    
    /// <summary>
    /// Gets or sets the average number of errors per day
    /// </summary>
    public double AverageErrorsPerDay { get; set; }
    
    /// <summary>
    /// Gets or sets the peak number of errors in a single day
    /// </summary>
    public int PeakErrorsInDay { get; set; }
    
    /// <summary>
    /// Gets or sets the date when peak errors occurred
    /// </summary>
    public DateTime? PeakErrorDate { get; set; }
    
    /// <summary>
    /// Gets or sets the percentage change compared to the previous period
    /// </summary>
    public double PercentageChange { get; set; }
    
    /// <summary>
    /// Gets a human-readable description of the trend direction
    /// </summary>
    public string TrendDescription => GetTrendDescription();

    /// <summary>
    /// Gets a descriptive text for the trend direction
    /// </summary>
    /// <returns>A string describing whether the trend is increasing, decreasing, or stable</returns>
    private string GetTrendDescription()
    {
        return TrendDirection switch
        {
            > 0.1 => "Increasing",
            < -0.1 => "Decreasing",
            _ => "Stable"
        };
    }
}

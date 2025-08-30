namespace FileChunkingSystem.Application.Models;

public class ErrorTrend : BaseModel
{
    public string ErrorType { get; set; } = string.Empty;
    public List<ErrorDataPoint> DataPoints { get; set; } = new();
    public double TrendDirection { get; set; } // Positive = increasing, Negative = decreasing
    public double AverageErrorsPerDay { get; set; }
    public int PeakErrorsInDay { get; set; }
    public DateTime? PeakErrorDate { get; set; }
    public double PercentageChange { get; set; } // Compared to previous period
    public string TrendDescription => GetTrendDescription();

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

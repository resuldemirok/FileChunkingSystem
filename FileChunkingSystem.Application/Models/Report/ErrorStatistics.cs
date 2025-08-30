namespace FileChunkingSystem.Application.Models;

public class ErrorStatistics : BaseModel
{
    public int TotalErrors { get; set; }
    public Dictionary<string, int> ErrorsByType { get; set; } = new();
    public Dictionary<string, int> ErrorsByOperation { get; set; } = new();
    public Dictionary<DateTime, int> ErrorsByDate { get; set; } = new();
    public Dictionary<string, int> ErrorsByFileType { get; set; } = new();
    public Dictionary<string, int> ErrorsByStorageType { get; set; } = new();
    public List<ErrorDetail> RecentErrors { get; set; } = new();
    public List<ErrorDetail> CriticalErrors { get; set; } = new();
    public double ErrorRate { get; set; }
    public DateTime? FirstErrorAt { get; set; }
    public DateTime? LastErrorAt { get; set; }
    public TimeSpan? AverageTimeBetweenErrors { get; set; }
    public Dictionary<string, ErrorTrend> ErrorTrends { get; set; } = new();
}

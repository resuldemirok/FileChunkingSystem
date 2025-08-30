namespace FileChunkingSystem.Application.Models;

public class ErrorDataPoint : BaseModel
{
    public DateTime Date { get; set; }
    public int ErrorCount { get; set; }
    public double ErrorRate { get; set; }
    public Dictionary<string, int>? ErrorBreakdown { get; set; }
}

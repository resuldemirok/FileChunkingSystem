namespace FileChunkingSystem.Application.Models;

public class TopFileInfo : BaseModel
{
    public Guid FileId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public int ChunkCount { get; set; }
    public string? ChunkingStrategy { get; set; }
    public DateTime ProcessedAt { get; set; }
    public TimeSpan ProcessingTime { get; set; }
    public string? MimeType { get; set; }
    public string Status { get; set; } = string.Empty;
    public long TotalDownloads { get; set; }
    public DateTime? LastAccessedAt { get; set; }
    public double AverageChunkSize => ChunkCount > 0 ? (double)FileSize / ChunkCount : 0;
    public string? StorageType { get; set; }
    public bool HasErrors { get; set; }
    public int ErrorCount { get; set; }
    public Dictionary<string, object>? AdditionalMetrics { get; set; }
}

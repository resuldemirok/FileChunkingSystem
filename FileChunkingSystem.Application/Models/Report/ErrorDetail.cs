namespace FileChunkingSystem.Application.Models;

public class ErrorDetail : BaseModel
{
    public Guid? FileId { get; set; }
    public string? FileName { get; set; }
    public string ErrorType { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
    public string? StackTrace { get; set; }
    public string Operation { get; set; } = string.Empty;
    public DateTime OccurredAt { get; set; }
    public string Severity { get; set; } = "Medium"; // Low, Medium, High, Critical
    public bool IsResolved { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public string? Resolution { get; set; }
    public Dictionary<string, object>? Context { get; set; }
    public int? ChunkIndex { get; set; }
    public string? StorageType { get; set; }
    public long? FileSize { get; set; }
    public string? UserAgent { get; set; }
    public string? IpAddress { get; set; }
    public int RetryCount { get; set; }
    public bool CanRetry { get; set; } = true;
}

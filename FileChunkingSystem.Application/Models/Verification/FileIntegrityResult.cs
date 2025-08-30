namespace FileChunkingSystem.Application.Models;

public class FileIntegrityResult : BaseModel
{
    public bool IsValid { get; set; }
    public int TotalChunks { get; set; }
    public int ValidChunks { get; set; }
    public List<int> CorruptedChunks { get; set; } = new();
    public List<int> MissingChunks { get; set; } = new();
    public string? ErrorMessage { get; set; }
    public DateTime CheckedAt { get; set; } = DateTime.UtcNow;
    public TimeSpan CheckDuration { get; set; }
    public Dictionary<int, string>? ChunkErrors { get; set; }
}

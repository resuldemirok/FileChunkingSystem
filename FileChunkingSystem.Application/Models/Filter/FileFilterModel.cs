namespace FileChunkingSystem.Application.Models;

public class FileFilterModel : BaseModel
{
    public long? MinSize { get; set; }
    public long? MaxSize { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? FileExtension { get; set; }
    public int? MinChunkCount { get; set; }
    public int? MaxChunkCount { get; set; }
    public string? Status { get; set; }
    public string? MimeType { get; set; }
    public bool? HasErrors { get; set; }
    public DateTime? LastAccessedBefore { get; set; }
    public DateTime? LastAccessedAfter { get; set; }
    public List<string>? Tags { get; set; }
    public Dictionary<string, string>? CustomMetadata { get; set; }
}

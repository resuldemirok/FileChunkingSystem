using FileChunkingSystem.Domain.Enums;

namespace FileChunkingSystem.Application.Models;

public class FileMetadataModel : BaseModel
{
    public string OriginalFileName { get; set; } = string.Empty;
    public string FileExtension { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string ChecksumSha256 { get; set; } = string.Empty;
    public ChunkingAlgorithm ChunkingAlgorithm { get; set; }
    public int TotalChunks { get; set; }
    public int ChunkSize { get; set; }
    public List<ChunkMetadataModel> Chunks { get; set; } = new();
}

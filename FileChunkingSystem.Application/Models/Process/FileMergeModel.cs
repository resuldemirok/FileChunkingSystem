namespace FileChunkingSystem.Application.Models;

public class FileMergeModel : BaseModel
{
    public Guid FileMetadataId { get; set; }
    public string OutputPath { get; set; } = string.Empty;
    public bool OverwriteIfExists { get; set; } = false;
}

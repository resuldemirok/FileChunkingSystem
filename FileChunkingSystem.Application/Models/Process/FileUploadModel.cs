namespace FileChunkingSystem.Application.Models;

public class FileUploadModel : BaseModel
{
    public List<FileInputModel> Files { get; set; } = new();
    public int? ChunkSize { get; set; }
}

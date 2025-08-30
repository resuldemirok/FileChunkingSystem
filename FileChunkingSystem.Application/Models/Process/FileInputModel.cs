namespace FileChunkingSystem.Application.Models;

public class FileInputModel : BaseModel
{
    public string FileName { get; set; } = string.Empty;
    public byte[] Content { get; set; } = Array.Empty<byte>();
}

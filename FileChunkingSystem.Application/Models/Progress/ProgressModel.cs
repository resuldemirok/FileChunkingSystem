namespace FileChunkingSystem.Application.Models;

public class ProgressModel : BaseModel
{
    public int CurrentStep { get; set; }
    public int TotalSteps { get; set; }
    public string Operation { get; set; } = string.Empty;
    public double PercentageComplete => TotalSteps > 0 ? (double)CurrentStep / TotalSteps * 100 : 0;
}

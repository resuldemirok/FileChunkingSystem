namespace FileChunkingSystem.Application.Models;

/// <summary>
/// Model representing progress information for long-running operations
/// </summary>
public class ProgressModel : BaseModel
{
    /// <summary>
    /// Gets or sets the current step number
    /// </summary>
    public int CurrentStep { get; set; }
    
    /// <summary>
    /// Gets or sets the total number of steps
    /// </summary>
    public int TotalSteps { get; set; }
    
    /// <summary>
    /// Gets or sets the description of the current operation
    /// </summary>
    public string Operation { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets the percentage completion of the operation
    /// </summary>
    public double PercentageComplete => TotalSteps > 0 ? (double)CurrentStep / TotalSteps * 100 : 0;
}

namespace FileChunkingSystem.Application.Models;

/// <summary>
/// Base model class providing common properties for all application models
/// </summary>
public abstract class BaseModel
{
    /// <summary>
    /// Gets or sets the unique identifier for the model
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();
    
    /// <summary>
    /// Gets or sets the creation timestamp
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Gets or sets the last update timestamp
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
    
    /// <summary>
    /// Gets or sets a value indicating whether the model is marked as deleted
    /// </summary>
    public bool IsDeleted { get; set; } = false;
}

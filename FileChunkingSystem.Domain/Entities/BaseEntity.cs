namespace FileChunkingSystem.Domain.Entities;

/// <summary>
/// Base entity class providing common properties for all domain entities
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// Gets or sets the unique identifier for the entity
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
    /// Gets or sets a value indicating whether the entity is marked as deleted
    /// </summary>
    public bool IsDeleted { get; set; } = false;
}

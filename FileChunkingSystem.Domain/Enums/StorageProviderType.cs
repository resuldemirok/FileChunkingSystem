namespace FileChunkingSystem.Domain.Enums;

/// <summary>
/// Defines the available storage provider types for chunk storage
/// </summary>
public enum StorageProviderType
{
    /// <summary>
    /// File system-based storage provider
    /// </summary>
    FileSystem = 1,
    
    /// <summary>
    /// PostgreSQL database storage provider
    /// </summary>
    PostgreSQL = 2,
    
    /// <summary>
    /// MongoDB document database storage provider
    /// </summary>
    MongoDB = 3
}

namespace FileChunkingSystem.Domain.Enums;

/// <summary>
/// Defines the available file chunking algorithms
/// </summary>
public enum ChunkingAlgorithm
{
    /// <summary>
    /// Round-robin distribution of chunks across storage providers
    /// </summary>
    RoundRobin = 1,
    
    /// <summary>
    /// Custom interleaved chunking pattern for optimized distribution
    /// </summary>
    CustomInterleaved = 2,
    
    /// <summary>
    /// Sequential block-based chunking for linear file processing
    /// </summary>
    SequentialBlocks = 3
}

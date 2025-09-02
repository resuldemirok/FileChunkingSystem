using FileChunkingSystem.Domain.Enums;

namespace FileChunkingSystem.Domain.Interfaces;

/// <summary>
/// Interface for file chunking strategies that define how files are split and merged
/// </summary>
public interface IChunkingStrategy
{
    /// <summary>
    /// Gets the chunking algorithm type used by this strategy
    /// </summary>
    ChunkingAlgorithm Algorithm { get; }
    
    /// <summary>
    /// Splits file data into chunks using the specified chunk size
    /// </summary>
    /// <param name="fileData">The file data to chunk</param>
    /// <param name="chunkSize">The size of each chunk in bytes</param>
    /// <returns>An array of byte arrays representing the file chunks</returns>
    byte[][] ChunkFile(byte[] fileData, int chunkSize);
    
    /// <summary>
    /// Merges file chunks back into the original file data
    /// </summary>
    /// <param name="chunks">The file chunks to merge</param>
    /// <returns>The merged file data</returns>
    byte[] MergeChunks(byte[][] chunks);
}

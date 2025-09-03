using FileChunkingSystem.Domain.Enums;
using FileChunkingSystem.Domain.Interfaces;

namespace FileChunkingSystem.Infrastructure.Strategies;

/// <summary>
/// Custom interleaved chunking strategy that distributes file bytes across chunks in an interleaved pattern.
/// This strategy improves fault tolerance by ensuring each chunk contains data from different file positions.
/// </summary>
public class CustomInterleavedChunkingStrategy : IChunkingStrategy
{
    /// <summary>
    /// Gets the chunking algorithm type
    /// </summary>
    public ChunkingAlgorithm Algorithm => ChunkingAlgorithm.CustomInterleaved;

    /// <summary>
    /// Chunks the file data using custom interleaved pattern
    /// </summary>
    /// <param name="fileData">The file data to chunk</param>
    /// <param name="chunkSize">The target size for each chunk</param>
    /// <returns>Array of chunk byte arrays</returns>
    public byte[][] ChunkFile(byte[] fileData, int chunkSize)
    {
        var totalChunks = (int)Math.Ceiling((double)fileData.Length / chunkSize);
        var chunks = new byte[totalChunks][];
        var chunkIndex = 0;
        
        // Interleaved pattern: take bytes from different positions
        for (int i = 0; i < totalChunks; i++)
        {
            var chunk = new List<byte>();
            var startPos = i;
            
            // Collect bytes in interleaved pattern by jumping across the file
            while (chunk.Count < chunkSize && startPos < fileData.Length)
            {
                chunk.Add(fileData[startPos]);
                startPos += totalChunks; // Jump by total chunks to create interleaved pattern
            }
            
            chunks[chunkIndex++] = chunk.ToArray();
        }
        
        return chunks;
    }

    /// <summary>
    /// Merges interleaved chunks back into the original file data
    /// </summary>
    /// <param name="chunks">The chunk byte arrays to merge</param>
    /// <returns>The reconstructed file data</returns>
    public byte[] MergeChunks(byte[][] chunks)
    {
        var totalLength = chunks.Sum(chunk => chunk.Length);
        var result = new byte[totalLength];
        var resultIndex = 0;
        var chunkPointers = new int[chunks.Length]; // Track position in each chunk
        
        // Reconstruct interleaved data by cycling through chunks
        while (resultIndex < totalLength)
        {
            // Take one byte from each chunk in sequence
            for (int chunkIndex = 0; chunkIndex < chunks.Length && resultIndex < totalLength; chunkIndex++)
            {
                if (chunkPointers[chunkIndex] < chunks[chunkIndex].Length)
                {
                    result[resultIndex++] = chunks[chunkIndex][chunkPointers[chunkIndex]++];
                }
            }
        }
        
        return result;
    }
}

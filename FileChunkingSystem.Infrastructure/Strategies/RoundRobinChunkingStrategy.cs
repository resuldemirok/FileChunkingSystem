using FileChunkingSystem.Domain.Enums;
using FileChunkingSystem.Domain.Interfaces;

namespace FileChunkingSystem.Infrastructure.Strategies;

/// <summary>
/// Round robin chunking strategy that distributes file bytes across chunks in a circular fashion.
/// Each byte is assigned to the next chunk in sequence, cycling back to the first chunk.
/// </summary>
public class RoundRobinChunkingStrategy : IChunkingStrategy
{
    /// <summary>
    /// Gets the chunking algorithm type
    /// </summary>
    public ChunkingAlgorithm Algorithm => ChunkingAlgorithm.RoundRobin;

    /// <summary>
    /// Chunks the file data using round robin distribution
    /// </summary>
    /// <param name="fileData">The file data to chunk</param>
    /// <param name="chunkSize">The target size for each chunk</param>
    /// <returns>Array of chunk byte arrays</returns>
    public byte[][] ChunkFile(byte[] fileData, int chunkSize)
    {
        var totalChunks = (int)Math.Ceiling((double)fileData.Length / chunkSize);
        var chunks = new List<byte[]>(totalChunks);

        // Create buffer for each chunk with maximum possible size
        for (int i = 0; i < totalChunks; i++)
            chunks.Add(new byte[chunkSize]);

        // Round Robin Distribution - assign each byte to chunks cyclically
        for (int i = 0; i < fileData.Length; i++)
        {
            int chunkIndex = i % totalChunks;        // Selected chunk (round robin)
            int position = i / totalChunks;          // Position in the chunk
            if (position < chunkSize)
                chunks[chunkIndex][position] = fileData[i];
        }

        // Adjust the size of each chunk to actual data length
        for (int i = 0; i < chunks.Count; i++)
        {
            int realSize = (fileData.Length - i + totalChunks - 1) / totalChunks;
            chunks[i] = chunks[i].Take(realSize).ToArray();
        }

        return chunks.ToArray();
    }

    /// <summary>
    /// Merges round robin distributed chunks back into the original file data
    /// </summary>
    /// <param name="chunks">The chunk byte arrays to merge</param>
    /// <returns>The reconstructed file data</returns>
    public byte[] MergeChunks(byte[][] chunks)
    {
        var totalLength = chunks.Sum(c => c.Length);
        var result = new byte[totalLength];

        int index = 0;
        int maxLength = chunks.Max(c => c.Length);

        // Round Robin Merging - take bytes from each chunk position by position
        for (int pos = 0; pos < maxLength; pos++)
        {
            // For each position, cycle through all chunks
            for (int chunkIndex = 0; chunkIndex < chunks.Length; chunkIndex++)
            {
                if (pos < chunks[chunkIndex].Length)
                {
                    result[index++] = chunks[chunkIndex][pos];
                }
            }
        }

        return result;
    }
}

using FileChunkingSystem.Domain.Enums;
using FileChunkingSystem.Domain.Interfaces;

namespace FileChunkingSystem.Infrastructure.Strategies;

/// <summary>
/// Sequential blocks chunking strategy that divides file data into sequential chunks with block patterns.
/// Creates chunks by copying data in sequential blocks, providing better data distribution.
/// </summary>
public class SequentialBlocksChunkingStrategy : IChunkingStrategy
{
    /// <summary>
    /// Gets the chunking algorithm type
    /// </summary>
    public ChunkingAlgorithm Algorithm => ChunkingAlgorithm.SequentialBlocks;

    /// <summary>
    /// Chunks the file data into sequential blocks with internal block patterns
    /// </summary>
    /// <param name="fileData">The file data to chunk</param>
    /// <param name="chunkSize">The size for each chunk</param>
    /// <returns>Array of chunk byte arrays</returns>
    public byte[][] ChunkFile(byte[] fileData, int chunkSize)
    {
        var chunks = new List<byte[]>();
        var blockSize = chunkSize / 4; // Create 4 blocks per chunk
        
        // Process file data in sequential chunks
        for (int i = 0; i < fileData.Length; i += chunkSize)
        {
            var remainingBytes = Math.Min(chunkSize, fileData.Length - i);
            var chunk = new byte[remainingBytes];
            
            // Copy data in sequential blocks with a pattern
            var sourceIndex = i;
            var destIndex = 0;
            
            // Fill chunk with data using block-based copying
            while (destIndex < remainingBytes)
            {
                var blockLength = Math.Min(blockSize, remainingBytes - destIndex);
                var actualBlockLength = Math.Min(blockLength, fileData.Length - sourceIndex);
                
                // Copy one block of data
                Array.Copy(fileData, sourceIndex, chunk, destIndex, actualBlockLength);
                sourceIndex += actualBlockLength;
                destIndex += actualBlockLength;
            }
            
            chunks.Add(chunk);
        }
        
        return chunks.ToArray();
    }

    /// <summary>
    /// Merges sequential chunks back into the original file data
    /// </summary>
    /// <param name="chunks">The chunk byte arrays to merge</param>
    /// <returns>The reconstructed file data</returns>
    public byte[] MergeChunks(byte[][] chunks)
    {
        var totalLength = chunks.Sum(chunk => chunk.Length);
        var result = new byte[totalLength];
        var offset = 0;
        
        // Concatenate all chunks sequentially
        foreach (var chunk in chunks)
        {
            Array.Copy(chunk, 0, result, offset, chunk.Length);
            offset += chunk.Length;
        }
        
        return result;
    }
}

using FileChunkingSystem.Domain.Enums;
using FileChunkingSystem.Domain.Interfaces;

namespace FileChunkingSystem.Infrastructure.Strategies;

public class CustomInterleavedChunkingStrategy : IChunkingStrategy
{
    public ChunkingAlgorithm Algorithm => ChunkingAlgorithm.CustomInterleaved;

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
            
            while (chunk.Count < chunkSize && startPos < fileData.Length)
            {
                chunk.Add(fileData[startPos]);
                startPos += totalChunks; // Jump by total chunks to create interleaved pattern
            }
            
            chunks[chunkIndex++] = chunk.ToArray();
        }
        
        return chunks;
    }

    public byte[] MergeChunks(byte[][] chunks)
    {
        var totalLength = chunks.Sum(chunk => chunk.Length);
        var result = new byte[totalLength];
        var resultIndex = 0;
        var chunkPointers = new int[chunks.Length];
        
        // Reconstruct interleaved data
        while (resultIndex < totalLength)
        {
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

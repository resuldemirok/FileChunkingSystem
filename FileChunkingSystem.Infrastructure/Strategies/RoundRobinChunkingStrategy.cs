using FileChunkingSystem.Domain.Enums;
using FileChunkingSystem.Domain.Interfaces;

namespace FileChunkingSystem.Infrastructure.Strategies;

public class RoundRobinChunkingStrategy : IChunkingStrategy
{
    public ChunkingAlgorithm Algorithm => ChunkingAlgorithm.RoundRobin;

    public byte[][] ChunkFile(byte[] fileData, int chunkSize)
    {
        var totalChunks = (int)Math.Ceiling((double)fileData.Length / chunkSize);
        var chunks = new List<byte[]>(totalChunks);

        // Create buffer for each chunk
        for (int i = 0; i < totalChunks; i++)
            chunks.Add(new byte[chunkSize]);

        // Round Robin Distribution
        for (int i = 0; i < fileData.Length; i++)
        {
            int chunkIndex = i % totalChunks;        // Selected chunk
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

    public byte[] MergeChunks(byte[][] chunks)
    {
        var totalLength = chunks.Sum(c => c.Length);
        var result = new byte[totalLength];

        int index = 0;
        int maxLength = chunks.Max(c => c.Length);

        // Round Robin Merging
        for (int pos = 0; pos < maxLength; pos++)
        {
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

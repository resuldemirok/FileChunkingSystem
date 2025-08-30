using FileChunkingSystem.Domain.Enums;
using FileChunkingSystem.Domain.Interfaces;

namespace FileChunkingSystem.Infrastructure.Strategies;

public class SequentialBlocksChunkingStrategy : IChunkingStrategy
{
    public ChunkingAlgorithm Algorithm => ChunkingAlgorithm.SequentialBlocks;

    public byte[][] ChunkFile(byte[] fileData, int chunkSize)
    {
        var chunks = new List<byte[]>();
        var blockSize = chunkSize / 4; // Create 4 blocks per chunk
        
        for (int i = 0; i < fileData.Length; i += chunkSize)
        {
            var remainingBytes = Math.Min(chunkSize, fileData.Length - i);
            var chunk = new byte[remainingBytes];
            
            // Copy data in sequential blocks with a pattern
            var sourceIndex = i;
            var destIndex = 0;
            
            while (destIndex < remainingBytes)
            {
                var blockLength = Math.Min(blockSize, remainingBytes - destIndex);
                var actualBlockLength = Math.Min(blockLength, fileData.Length - sourceIndex);
                
                Array.Copy(fileData, sourceIndex, chunk, destIndex, actualBlockLength);
                sourceIndex += actualBlockLength;
                destIndex += actualBlockLength;
            }
            
            chunks.Add(chunk);
        }
        
        return chunks.ToArray();
    }

    public byte[] MergeChunks(byte[][] chunks)
    {
        var totalLength = chunks.Sum(chunk => chunk.Length);
        var result = new byte[totalLength];
        var offset = 0;
        
        foreach (var chunk in chunks)
        {
            Array.Copy(chunk, 0, result, offset, chunk.Length);
            offset += chunk.Length;
        }
        
        return result;
    }
}

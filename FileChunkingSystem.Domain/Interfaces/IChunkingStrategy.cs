using FileChunkingSystem.Domain.Enums;

namespace FileChunkingSystem.Domain.Interfaces;

public interface IChunkingStrategy
{
    ChunkingAlgorithm Algorithm { get; }
    byte[][] ChunkFile(byte[] fileData, int chunkSize);
    byte[] MergeChunks(byte[][] chunks);
}

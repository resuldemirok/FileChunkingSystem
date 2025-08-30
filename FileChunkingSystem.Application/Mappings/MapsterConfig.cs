using FileChunkingSystem.Domain.Entities;
using FileChunkingSystem.Application.Models;
using Mapster;

namespace FileChunkingSystem.Application.Mappings;

public static class MapsterConfig
{
    public static void RegisterMappings()
    {
        // FileMetadata <-> FileMetadataModel
        TypeAdapterConfig<FileMetadata, FileMetadataModel>.NewConfig()
            .Map(dest => dest.Chunks, src => src.Chunks);
        TypeAdapterConfig<FileMetadataModel, FileMetadata>.NewConfig()
            .Map(dest => dest.Chunks, src => src.Chunks);

        // ChunkMetadata <-> ChunkMetadataModel
        TypeAdapterConfig<ChunkMetadata, ChunkMetadataModel>.NewConfig();
        TypeAdapterConfig<ChunkMetadataModel, ChunkMetadata>.NewConfig();
    }
}

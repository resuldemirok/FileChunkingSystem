using FileChunkingSystem.Domain.Entities;
using FileChunkingSystem.Application.Models;
using Mapster;

namespace FileChunkingSystem.Application.Mappings;

/// <summary>
/// Configuration class for Mapster object mapping between domain entities and application models
/// </summary>
public static class MapsterConfig
{
    /// <summary>
    /// Registers all mapping configurations for the application
    /// </summary>
    public static void RegisterMappings()
    {
        // FileMetadata <-> FileMetadataModel mappings with chunk collections
        TypeAdapterConfig<FileMetadata, FileMetadataModel>.NewConfig()
            .Map(dest => dest.Chunks, src => src.Chunks);
        TypeAdapterConfig<FileMetadataModel, FileMetadata>.NewConfig()
            .Map(dest => dest.Chunks, src => src.Chunks);

        // ChunkMetadata <-> ChunkMetadataModel mappings
        TypeAdapterConfig<ChunkMetadata, ChunkMetadataModel>.NewConfig();
        TypeAdapterConfig<ChunkMetadataModel, ChunkMetadata>.NewConfig();
    }
}

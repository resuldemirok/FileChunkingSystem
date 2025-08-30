using FileChunkingSystem.Application.Mappings;
using FileChunkingSystem.Application.Models;
using FileChunkingSystem.Domain.Entities;
using FileChunkingSystem.Domain.Enums;
using FluentAssertions;
using Mapster;
using Xunit;

namespace FileChunkingSystem.Application.Tests.Mappings;

public class MapsterConfigTests
{
    public MapsterConfigTests()
    {
        MapsterConfig.RegisterMappings();
    }

    [Fact]
    public void Should_Map_FileMetadata_To_FileMetadataModel()
    {
        // Arrange
        var fileMetadata = new FileMetadata
        {
            Id = Guid.NewGuid(),
            OriginalFileName = "test.txt",
            FileExtension = ".txt",
            FileSize = 1024,
            ChecksumSha256 = "abc123",
            ChunkingAlgorithm = ChunkingAlgorithm.RoundRobin,
            TotalChunks = 2,
            ChunkSize = 512,
            Chunks = new List<ChunkMetadata>
            {
                new ChunkMetadata { ChunkIndex = 0 },
                new ChunkMetadata { ChunkIndex = 1 }
            }
        };

        // Act
        var model = fileMetadata.Adapt<FileMetadataModel>();

        // Assert
        model.Should().NotBeNull();
        model.Id.Should().Be(fileMetadata.Id);
        model.OriginalFileName.Should().Be(fileMetadata.OriginalFileName);
        model.FileExtension.Should().Be(fileMetadata.FileExtension);
        model.FileSize.Should().Be(fileMetadata.FileSize);
        model.ChecksumSha256.Should().Be(fileMetadata.ChecksumSha256);
        model.ChunkingAlgorithm.Should().Be(fileMetadata.ChunkingAlgorithm);
        model.TotalChunks.Should().Be(fileMetadata.TotalChunks);
        model.ChunkSize.Should().Be(fileMetadata.ChunkSize);
        model.Chunks.Should().HaveCount(2);
    }

    [Fact]
    public void Should_Map_FileMetadataModel_To_FileMetadata()
    {
        // Arrange
        var model = new FileMetadataModel
        {
            Id = Guid.NewGuid(),
            OriginalFileName = "test.txt",
            FileExtension = ".txt",
            FileSize = 1024,
            ChecksumSha256 = "abc123",
            ChunkingAlgorithm = ChunkingAlgorithm.RoundRobin,
            TotalChunks = 2,
            ChunkSize = 512,
            Chunks = new List<ChunkMetadataModel>
            {
                new ChunkMetadataModel { ChunkIndex = 0 },
                new ChunkMetadataModel { ChunkIndex = 1 }
            }
        };

        // Act
        var entity = model.Adapt<FileMetadata>();

        // Assert
        entity.Should().NotBeNull();
        entity.Id.Should().Be(model.Id);
        entity.OriginalFileName.Should().Be(model.OriginalFileName);
        entity.FileExtension.Should().Be(model.FileExtension);
        entity.FileSize.Should().Be(model.FileSize);
        entity.ChecksumSha256.Should().Be(model.ChecksumSha256);
        entity.ChunkingAlgorithm.Should().Be(model.ChunkingAlgorithm);
        entity.TotalChunks.Should().Be(model.TotalChunks);
        entity.ChunkSize.Should().Be(model.ChunkSize);
        entity.Chunks.Should().HaveCount(2);
    }

    [Fact]
    public void Should_Map_ChunkMetadata_To_ChunkMetadataModel()
    {
        // Arrange
        var chunkMetadata = new ChunkMetadata
        {
            Id = Guid.NewGuid(),
            FileMetadataId = Guid.NewGuid(),
            ChunkIndex = 1,
            ChunkSize = 512,
            ChecksumSha256 = "def456",
            StorageProviderType = StorageProviderType.PostgreSQL,
            StorageKey = "chunk-key"
        };

        // Act
        var model = chunkMetadata.Adapt<ChunkMetadataModel>();

        // Assert
        model.Should().NotBeNull();
        model.Id.Should().Be(chunkMetadata.Id);
        model.FileMetadataId.Should().Be(chunkMetadata.FileMetadataId);
        model.ChunkIndex.Should().Be(chunkMetadata.ChunkIndex);
        model.ChunkSize.Should().Be(chunkMetadata.ChunkSize);
        model.ChecksumSha256.Should().Be(chunkMetadata.ChecksumSha256);
        model.StorageProviderType.Should().Be(chunkMetadata.StorageProviderType);
        model.StorageKey.Should().Be(chunkMetadata.StorageKey);
    }
}

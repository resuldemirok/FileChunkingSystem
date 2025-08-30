using FileChunkingSystem.Domain.Entities;
using FileChunkingSystem.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace FileChunkingSystem.Domain.Tests.Entities;

public class ChunkMetadataTests
{
    [Fact]
    public void ChunkMetadata_ShouldInitializeWithDefaults()
    {
        // Arrange & Act
        var chunkMetadata = new ChunkMetadata();

        // Assert
        chunkMetadata.Id.Should().NotBeEmpty();
        chunkMetadata.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        chunkMetadata.IsDeleted.Should().BeFalse();
        chunkMetadata.FileMetadataId.Should().BeEmpty();
        chunkMetadata.ChunkIndex.Should().Be(0);
        chunkMetadata.ChunkSize.Should().Be(0);
        chunkMetadata.ChecksumSha256.Should().BeEmpty();
        chunkMetadata.StorageKey.Should().BeEmpty();
        chunkMetadata.ChunkData.Should().BeEmpty();
    }

    [Fact]
    public void ChunkMetadata_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var chunkMetadata = new ChunkMetadata();
        var expectedFileMetadataId = Guid.NewGuid();
        var expectedIndex = 1;
        var expectedSize = 512;
        var expectedChecksum = "def456";
        var expectedStorageType = StorageProviderType.PostgreSQL;
        var expectedStorageKey = "chunk-key-123";
        var expectedData = new byte[] { 1, 2, 3, 4, 5 };

        // Act
        chunkMetadata.FileMetadataId = expectedFileMetadataId;
        chunkMetadata.ChunkIndex = expectedIndex;
        chunkMetadata.ChunkSize = expectedSize;
        chunkMetadata.ChecksumSha256 = expectedChecksum;
        chunkMetadata.StorageProviderType = expectedStorageType;
        chunkMetadata.StorageKey = expectedStorageKey;
        chunkMetadata.ChunkData = expectedData;

        // Assert
        chunkMetadata.FileMetadataId.Should().Be(expectedFileMetadataId);
        chunkMetadata.ChunkIndex.Should().Be(expectedIndex);
        chunkMetadata.ChunkSize.Should().Be(expectedSize);
        chunkMetadata.ChecksumSha256.Should().Be(expectedChecksum);
        chunkMetadata.StorageProviderType.Should().Be(expectedStorageType);
        chunkMetadata.StorageKey.Should().Be(expectedStorageKey);
        chunkMetadata.ChunkData.Should().Equal(expectedData);
    }
}

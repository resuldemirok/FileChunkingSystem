using FileChunkingSystem.Domain.Entities;
using FileChunkingSystem.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace FileChunkingSystem.Domain.Tests.Entities;

public class FileMetadataTests
{
    [Fact]
    public void FileMetadata_ShouldInitializeWithDefaults()
    {
        // Arrange & Act
        var fileMetadata = new FileMetadata();

        // Assert
        fileMetadata.Id.Should().NotBeEmpty();
        fileMetadata.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        fileMetadata.IsDeleted.Should().BeFalse();
        fileMetadata.Chunks.Should().NotBeNull().And.BeEmpty();
        fileMetadata.OriginalFileName.Should().BeEmpty();
        fileMetadata.FileExtension.Should().BeEmpty();
        fileMetadata.ChecksumSha256.Should().BeEmpty();
    }

    [Fact]
    public void FileMetadata_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var fileMetadata = new FileMetadata();
        var expectedFileName = "test.txt";
        var expectedExtension = ".txt";
        var expectedSize = 1024L;
        var expectedChecksum = "abc123";
        var expectedAlgorithm = ChunkingAlgorithm.RoundRobin;
        var expectedChunks = 5;
        var expectedChunkSize = 256;

        // Act
        fileMetadata.OriginalFileName = expectedFileName;
        fileMetadata.FileExtension = expectedExtension;
        fileMetadata.FileSize = expectedSize;
        fileMetadata.ChecksumSha256 = expectedChecksum;
        fileMetadata.ChunkingAlgorithm = expectedAlgorithm;
        fileMetadata.TotalChunks = expectedChunks;
        fileMetadata.ChunkSize = expectedChunkSize;

        // Assert
        fileMetadata.OriginalFileName.Should().Be(expectedFileName);
        fileMetadata.FileExtension.Should().Be(expectedExtension);
        fileMetadata.FileSize.Should().Be(expectedSize);
        fileMetadata.ChecksumSha256.Should().Be(expectedChecksum);
        fileMetadata.ChunkingAlgorithm.Should().Be(expectedAlgorithm);
        fileMetadata.TotalChunks.Should().Be(expectedChunks);
        fileMetadata.ChunkSize.Should().Be(expectedChunkSize);
    }
}

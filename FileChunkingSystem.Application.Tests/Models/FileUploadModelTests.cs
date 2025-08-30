using FileChunkingSystem.Application.Models;
using FluentAssertions;
using Xunit;

namespace FileChunkingSystem.Application.Tests.Models;

public class FileUploadModelTests
{
    [Fact]
    public void FileUploadModel_ShouldInitializeWithDefaults()
    {
        // Arrange & Act
        var model = new FileUploadModel();

        // Assert
        model.Id.Should().NotBeEmpty();
        model.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        model.IsDeleted.Should().BeFalse();
        model.Files.Should().NotBeNull().And.BeEmpty();
        model.ChunkSize.Should().BeNull();
    }

    [Fact]
    public void FileUploadModel_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var model = new FileUploadModel();
        var files = new List<FileInputModel>
        {
            new FileInputModel { FileName = "test.txt" }
        };
        var chunkSize = 1024;

        // Act
        model.Files = files;
        model.ChunkSize = chunkSize;

        // Assert
        model.Files.Should().HaveCount(1);
        model.ChunkSize.Should().Be(chunkSize);
    }
}

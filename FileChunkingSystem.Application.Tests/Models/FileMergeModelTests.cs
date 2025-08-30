using FileChunkingSystem.Application.Models;
using FluentAssertions;
using Xunit;

namespace FileChunkingSystem.Application.Tests.Models;

public class FileMergeModelTests
{
    [Fact]
    public void FileMergeModel_ShouldInitializeWithDefaults()
    {
        // Arrange & Act
        var model = new FileMergeModel();

        // Assert
        model.Id.Should().NotBeEmpty();
        model.FileMetadataId.Should().BeEmpty();
        model.OutputPath.Should().BeEmpty();
        model.OverwriteIfExists.Should().BeFalse();
    }

    [Fact]
    public void FileMergeModel_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var model = new FileMergeModel();
        var fileId = Guid.NewGuid();
        var outputPath = "/path/to/output.txt";

        // Act
        model.FileMetadataId = fileId;
        model.OutputPath = outputPath;
        model.OverwriteIfExists = true;

        // Assert
        model.FileMetadataId.Should().Be(fileId);
        model.OutputPath.Should().Be(outputPath);
        model.OverwriteIfExists.Should().BeTrue();
    }
}

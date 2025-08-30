using FileChunkingSystem.Application.Models;
using FluentAssertions;
using Xunit;

namespace FileChunkingSystem.Application.Tests.Models;

public class FileIntegrityResultTests
{
    [Fact]
    public void FileIntegrityResult_ShouldInitializeWithDefaults()
    {
        // Arrange & Act
        var result = new FileIntegrityResult();

        // Assert
        result.Id.Should().NotBeEmpty();
        result.IsValid.Should().BeFalse();
        result.TotalChunks.Should().Be(0);
        result.ValidChunks.Should().Be(0);
        result.CorruptedChunks.Should().NotBeNull().And.BeEmpty();
        result.MissingChunks.Should().NotBeNull().And.BeEmpty();
        result.CheckedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void FileIntegrityResult_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var result = new FileIntegrityResult();
        var corruptedChunks = new List<int> { 1, 3 };
        var missingChunks = new List<int> { 2 };
        var errorMessage = "File corrupted";
        var checkDuration = TimeSpan.FromSeconds(5);

        // Act
        result.IsValid = false;
        result.TotalChunks = 5;
        result.ValidChunks = 2;
        result.CorruptedChunks = corruptedChunks;
        result.MissingChunks = missingChunks;
        result.ErrorMessage = errorMessage;
        result.CheckDuration = checkDuration;

        // Assert
        result.IsValid.Should().BeFalse();
        result.TotalChunks.Should().Be(5);
        result.ValidChunks.Should().Be(2);
        result.CorruptedChunks.Should().Equal(corruptedChunks);
        result.MissingChunks.Should().Equal(missingChunks);
        result.ErrorMessage.Should().Be(errorMessage);
        result.CheckDuration.Should().Be(checkDuration);
    }
}

using FileChunkingSystem.Application.Models;
using FluentAssertions;
using Xunit;

namespace FileChunkingSystem.Application.Tests.Models;

public class ProgressModelTests
{
    [Fact]
    public void ProgressModel_ShouldCalculatePercentageCorrectly()
    {
        // Arrange
        var model = new ProgressModel
        {
            CurrentStep = 50,
            TotalSteps = 100,
            Operation = "Processing"
        };

        // Act & Assert
        model.PercentageComplete.Should().Be(50.0);
    }

    [Fact]
    public void ProgressModel_ShouldReturnZeroPercentage_WhenTotalStepsIsZero()
    {
        // Arrange
        var model = new ProgressModel
        {
            CurrentStep = 10,
            TotalSteps = 0
        };

        // Act & Assert
        model.PercentageComplete.Should().Be(0.0);
    }

    [Fact]
    public void ProgressModel_ShouldHandle100Percent()
    {
        // Arrange
        var model = new ProgressModel
        {
            CurrentStep = 100,
            TotalSteps = 100
        };

        // Act & Assert
        model.PercentageComplete.Should().Be(100.0);
    }
}

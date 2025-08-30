using FileChunkingSystem.Application.Models;
using FluentAssertions;
using System.Text;
using Xunit;

namespace FileChunkingSystem.Application.Tests.Models;

public class FileInputModelTests
{
    [Fact]
    public void FileInputModel_ShouldInitializeWithDefaults()
    {
        // Arrange & Act
        var model = new FileInputModel();

        // Assert
        model.Id.Should().NotBeEmpty();
        model.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        model.IsDeleted.Should().BeFalse();
        model.FileName.Should().BeEmpty();
        model.Content.Should().BeEmpty();
    }

    [Fact]
    public void FileInputModel_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var model = new FileInputModel();
        var fileName = "test.txt";
        var content = Encoding.UTF8.GetBytes("Hello World");

        // Act
        model.FileName = fileName;
        model.Content = content;

        // Assert
        model.FileName.Should().Be(fileName);
        model.Content.Should().Equal(content);
    }
}

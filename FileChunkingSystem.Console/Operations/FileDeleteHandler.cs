using FileChunkingSystem.Application.Interfaces;
using FileChunkingSystem.Console.Handlers.Abstract;
using Microsoft.Extensions.Logging;
using Spectre.Console;

namespace FileChunkingSystem.Console.Handlers;

public class FileDeleteHandler : BaseHandler, IConsoleHandler
{
    public FileDeleteHandler(
        IFileChunkingService fileChunkingService,
        IPerformanceTrackingService performanceTrackingService,
        ILogger<FileDeleteHandler> logger)
        : base(fileChunkingService, performanceTrackingService, logger)
    {
    }

    public async Task HandleAsync()
    {
        try
        {
            var fileId = GetFileId();
            
            if (fileId == null)
            {
                ShowWarning("Delete operation cancelled.");
                return;
            }
            else if (!GetDeleteConfirmation())
            {
                ShowWarning("Delete operation cancelled.");
                return;
            }

            var deleted = await FileChunkingService.DeleteFileAsync(fileId.Value);

            if (deleted)
                ShowSuccess("File deleted successfully!");
            else
                ShowError("File not found!");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error during file deletion");
            ShowError($"Delete failed: {ex.Message}");
            throw;
        }
    }

    private Guid? GetFileId()
    {
        while (true)
        {
            var fileIdInput = AnsiConsole.Prompt(
                new TextPrompt<string>("Enter file ID to delete (or type [red]q[/] to cancel):")
                    .AllowEmpty()
            );

            if (string.Equals(fileIdInput, "q", StringComparison.OrdinalIgnoreCase))
                return null;

            if (string.IsNullOrWhiteSpace(fileIdInput))
                return Guid.Empty;

            if (Guid.TryParse(fileIdInput, out var guid))
                return guid;

            AnsiConsole.MarkupLine("[red]Invalid GUID format! Please enter a valid GUID or 'q' to cancel.[/]");
        }
    }

    private bool GetDeleteConfirmation()
    {
        return AnsiConsole.Confirm("Are you sure you want to delete this file?");
    }
}

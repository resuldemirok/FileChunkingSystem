using FileChunkingSystem.Application.Interfaces;
using FileChunkingSystem.Application.Models;
using FileChunkingSystem.Console.Handlers.Abstract;
using Microsoft.Extensions.Logging;
using Spectre.Console;

namespace FileChunkingSystem.Console.Handlers;

public class FileMergeHandler : BaseHandler, IConsoleHandler
{
    public FileMergeHandler(
        IFileChunkingService fileChunkingService,
        IPerformanceTrackingService performanceTrackingService,
        ILogger<FileMergeHandler> logger)
        : base(fileChunkingService, performanceTrackingService, logger)
    {
    }

    public async Task HandleAsync()
    {
        try
        {
            var fileId = GetFileId();

            if (fileId is null)
            {
                ShowInfo("Merge cancelled.");
                return;
            }
            else if (fileId == Guid.Empty)
            {
                ShowWarning("Invalid input. Merge cancelled.");
                return;
            }

            var outputPath = GetOutputPath();

            if (outputPath is null)
            {
                ShowInfo("Merge cancelled.");
                return;
            }
            else if (outputPath == string.Empty)
            {
                ShowWarning("Invalid input. Merge cancelled.");
                return;
            }

            var overwrite = !File.Exists(outputPath) || GetOverwriteConfirmation();

            if (!overwrite) {
                ShowInfo("Merge cancelled.");
                return;
            }

            var model = CreateMergeModel(fileId.Value, outputPath, overwrite);
            var finalPath = await ProcessFileMerge(model);

            ShowSuccess("File merged and saved successfully!");
            ShowInfo($"File saved to: {finalPath}");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error during file merge");
            ShowError($"Merge failed: {ex.Message}");
            throw;
        }
    }

    private Guid? GetFileId()
    {
        var fileIdInput = AnsiConsole.Prompt(
            new TextPrompt<string>("Enter file ID (or type [red]q[/] to cancel):")
                .AllowEmpty()
                .Validate(id =>
                {
                    if (string.IsNullOrWhiteSpace(id) || id.Equals("q", StringComparison.OrdinalIgnoreCase))
                        return ValidationResult.Success();

                    return Guid.TryParse(id, out _)
                        ? ValidationResult.Success()
                        : ValidationResult.Error("Invalid GUID format!");
                })
        );

        if (fileIdInput.Equals("q", StringComparison.OrdinalIgnoreCase))
            return null;

        if (string.IsNullOrWhiteSpace(fileIdInput))
            return Guid.Empty;

        return Guid.Parse(fileIdInput);
    }

    private string? GetOutputPath()
    {
        var outputPath = AnsiConsole.Prompt(
            new TextPrompt<string>("Enter output file path (or type [red]q[/] to cancel):")
                .AllowEmpty()
        );

        if (outputPath.Equals("q", StringComparison.OrdinalIgnoreCase))
            return null;

        if (string.IsNullOrWhiteSpace(outputPath))
            return string.Empty;

        return outputPath;
    }

    private bool GetOverwriteConfirmation()
    {
        return AnsiConsole.Confirm("This file is exists. Do you want to overwrite it?", false);
    }

    private FileMergeModel CreateMergeModel(Guid fileId, string outputPath, bool overwrite)
    {
        if (!Directory.Exists("restored"))
            Directory.CreateDirectory("restored");

        return new FileMergeModel
        {
            FileMetadataId = fileId,
            OutputPath = Path.Combine("restored", outputPath),
            OverwriteIfExists = overwrite
        };
    }

    private async Task<string> ProcessFileMerge(FileMergeModel model)
    {
        var progress = new Progress<ProgressModel>(p =>
        {
            AnsiConsole.MarkupLine($"[green]{p.Operation}[/] - {p.PercentageComplete:F1}%");
        });

        return await AnsiConsole.Progress()
            .StartAsync(async ctx =>
            {
                var task = ctx.AddTask("Merging file...");
                var mergedData = await FileChunkingService.MergeAndRetrieveFileAsync(model, progress);
                
                var finalPath = model.OutputPath;
                if (File.Exists(finalPath) && !model.OverwriteIfExists)
                {
                    finalPath = Path.Combine(
                        Path.GetDirectoryName(finalPath)!,
                        $"{Path.GetFileNameWithoutExtension(finalPath)}_merged{Path.GetExtension(finalPath)}");
                }

                await File.WriteAllBytesAsync(finalPath, mergedData);
                
                task.MaxValue = 100;
                task.Value = 100;

                return finalPath;
            });
    }
}

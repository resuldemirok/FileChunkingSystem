using FileChunkingSystem.Application.Interfaces;
using FileChunkingSystem.Application.Models;
using FileChunkingSystem.Console.Handlers.Abstract;
using Microsoft.Extensions.Logging;
using Spectre.Console;

namespace FileChunkingSystem.Console.Handlers;

public class FileUploadHandler : BaseHandler, IConsoleHandler
{
    public FileUploadHandler(
        IFileChunkingService fileChunkingService,
        IPerformanceTrackingService performanceTrackingService,
        ILogger<FileUploadHandler> logger)
        : base(fileChunkingService, performanceTrackingService, logger)
    {
    }

    public async Task HandleAsync()
    {
        try
        {
            var filePaths = GetFilePathsWithMenu();

            if (filePaths == null)
            {
                ShowInfo("Upload cancelled.");
                return;
            }

            var chunkSize = GetChunkSize();

            if (chunkSize == null)
            {
                ShowInfo("Upload cancelled.");
                return;
            }
            
            foreach (var filePath in filePaths)
            {
                var fileContent = await File.ReadAllBytesAsync(filePath);
                var fileName = Path.GetFileName(filePath);

                var model = CreateUploadModel(fileName, fileContent, chunkSize == 0 ? null : chunkSize);
                var fileIds = await ProcessFileUpload(model);

                ShowSuccess("File chunked and stored successfully!");

                foreach (var fileId in fileIds)
                {
                    AnsiConsole.MarkupLine($"[blue]File ID:[/] {fileId}");
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error during file upload");
            ShowError($"Upload failed: {ex.Message}");
            throw;
        }
    }

    private enum MenuFileUploadActions
    {
        AddFile,
        StartProcess,
        Cancel
    }

    private static Dictionary<string, MenuFileUploadActions> CreateMenuFileUploadActions()
    {
        return new Dictionary<string, MenuFileUploadActions>
        {
            ["Add file"] = MenuFileUploadActions.AddFile,
            ["Start process"] = MenuFileUploadActions.StartProcess,
            ["Cancel"] = MenuFileUploadActions.Cancel
        };
    }

    private static List<string>? GetFilePathsWithMenu()
    {
        var filePaths = new List<string>();
        var menuFileUploadActions = CreateMenuFileUploadActions();

        while (true)
        {
            AnsiConsole.MarkupLine("\n[bold]Current files:[/]");
            if (filePaths.Any())
            {
                foreach (var f in filePaths)
                    AnsiConsole.MarkupLine($" - {f}");
            }
            else
                AnsiConsole.MarkupLine(" [grey]No files added yet.[/]");

            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Select an action:")
                    .AddChoices(menuFileUploadActions.Keys)
            );

            switch (menuFileUploadActions[choice])
            {
                case MenuFileUploadActions.AddFile:
                    var input = AnsiConsole.Prompt(
                        new TextPrompt<string>("Enter a file path (or type [red]q[/] to cancel):")
                            .AllowEmpty()
                    );

                    if (string.Equals(input, "q", StringComparison.OrdinalIgnoreCase))
                    {
                        AnsiConsole.MarkupLine("[yellow]File add cancelled.[/]");
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(input))
                    {
                        AnsiConsole.MarkupLine("[yellow]Empty input, please enter a valid file path.[/]");
                        continue;
                    }

                    if (!File.Exists(input))
                    {
                        AnsiConsole.MarkupLine($"[red]File does not exist: {input}[/]");
                        continue;
                    }

                    filePaths.Add(input);
                    AnsiConsole.MarkupLine($"[green]Added:[/] {input}");
                    break;

                case MenuFileUploadActions.StartProcess:
                    if (!filePaths.Any())
                    {
                        AnsiConsole.MarkupLine("[yellow]No files added yet. Please add at least one file.[/]");
                        continue;
                    }
                    return filePaths;

                case MenuFileUploadActions.Cancel:
                    return null;
            }
        }
    }

    private int? GetChunkSize()
    {
        var input = AnsiConsole.Prompt(
            new TextPrompt<string>("Chunk size (KB, leave empty for auto calculation, or type [red]q[/] to cancel):")
                .AllowEmpty()
                .Validate(value =>
                {
                    if (string.IsNullOrWhiteSpace(value) || value.Equals("q", StringComparison.OrdinalIgnoreCase))
                        return ValidationResult.Success();

                    if (int.TryParse(value, out var number) && number > 0)
                        return ValidationResult.Success();

                    return ValidationResult.Error("Chunk size must be positive!");
                })
        );

        if (input.Equals("q", StringComparison.OrdinalIgnoreCase))
            return null;

        if (string.IsNullOrWhiteSpace(input))
            return 0;

        return int.Parse(input);
    }

    private FileUploadModel CreateUploadModel(string fileName, byte[] content, int? chunkSize)
    {
        return new FileUploadModel
        {
            Files = new List<FileInputModel>
            {
                new FileInputModel
                {
                    FileName = fileName,
                    Content = content
                }
            },
            ChunkSize = chunkSize * 1024 // Convert KB to bytes
        };
    }

    private async Task<List<Guid>> ProcessFileUpload(FileUploadModel model)
    {
        var progress = new Progress<ProgressModel>(p =>
        {
            AnsiConsole.MarkupLine($"[green]{p.Operation}[/] - {p.PercentageComplete:F1}%");
        });

        return await AnsiConsole.Progress()
            .StartAsync(async ctx =>
            {
                var task = ctx.AddTask("Processing file...");
                var fileIds = await FileChunkingService.ChunkAndStoreFileAsync(model, progress);
                
                task.MaxValue = 100;
                task.Value = 100;

                return fileIds;
            });
    }
}

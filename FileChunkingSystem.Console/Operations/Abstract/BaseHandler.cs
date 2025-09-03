using FileChunkingSystem.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Spectre.Console;

namespace FileChunkingSystem.Console.Handlers.Abstract;

/// <summary>
/// Abstract base class for console handlers providing common functionality and services.
/// </summary>
public abstract class BaseHandler
{
    protected readonly IFileChunkingService FileChunkingService;
    protected readonly IPerformanceTrackingService PerformanceTrackingService;
    protected readonly ILogger Logger;

    protected BaseHandler(
        IFileChunkingService fileChunkingService,
        IPerformanceTrackingService performanceTrackingService,
        ILogger logger)
    {
        FileChunkingService = fileChunkingService;
        PerformanceTrackingService = performanceTrackingService;
        Logger = logger;
    }

    protected bool? PromptYesNo(string message)
    {
        var input = AnsiConsole.Prompt(
            new TextPrompt<string>($"{message} ([green]y[/]/[red]n[/], type [red]q[/] to cancel):")
                .Validate(val =>
                {
                    if (string.Equals(val, "y", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(val, "n", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(val, "q", StringComparison.OrdinalIgnoreCase))
                        return ValidationResult.Success();

                    return ValidationResult.Error("Please enter 'y', 'n', or 'q'.");
                })
        );

        if (string.Equals(input, "q", StringComparison.OrdinalIgnoreCase))
            return null;
        return string.Equals(input, "y", StringComparison.OrdinalIgnoreCase);
    }

    protected string FormatBytes(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }

    protected void ShowSuccess(string message)
    {
        AnsiConsole.MarkupLine($"[green]{message}[/]");
    }

    protected void ShowError(string message)
    {
        AnsiConsole.MarkupLine($"[red]{message}[/]");
    }

    protected void ShowWarning(string message)
    {
        AnsiConsole.MarkupLine($"[yellow]{message}[/]");
    }

    protected void ShowInfo(string message)
    {
        AnsiConsole.MarkupLine($"[blue]{message}[/]");
    }
}

using FileChunkingSystem.Console.Handlers.Factory;
using FileChunkingSystem.Console.Helpers;
using FileChunkingSystem.Console.Enums;
using Microsoft.Extensions.Logging;
using Spectre.Console;

namespace FileChunkingSystem.Console.Services;

public class ConsoleApplication
{
    private readonly HandlerFactory _handlerFactory;
    private readonly ILogger<ConsoleApplication> _logger;

    public ConsoleApplication(
        HandlerFactory handlerFactory,
        ILogger<ConsoleApplication> logger)
    {
        _handlerFactory = handlerFactory;
        _logger = logger;
    }

    public async Task RunAsync()
    {
        ShowWelcomeMessage();

        var menuOptions = EnumHelpers.ToDictionary<MenuOptions>();

        while (true)
        {
            var choice = GetUserChoice(menuOptions);

            try
            {
                await ExecuteChoice(choice);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during operation");
                AnsiConsole.WriteException(ex);
                throw;
            }

            if (choice == MenuOptions.Exit)
                break;

            WaitForUserInput();
        }
    }

    private void ShowWelcomeMessage()
    {
        AnsiConsole.Write(
            new FigletText("File Chunking System")
                .LeftJustified()
                .Color(Color.Blue));
    }

    private MenuOptions GetUserChoice(Dictionary<string, MenuOptions> menuOptions)
    {
        var choice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("What would you like to do?")
                .AddChoices(menuOptions.Keys));

        return menuOptions[choice];
    }

    private async Task ExecuteChoice(MenuOptions choice)
    {
        switch (choice)
        {
            case MenuOptions.Upload:
                await _handlerFactory.CreateFileUploadHandler().HandleAsync();
                break;
            case MenuOptions.Merge:
                await _handlerFactory.CreateFileMergeHandler().HandleAsync();
                break;
            case MenuOptions.List:
                await _handlerFactory.CreateFileListHandler().HandleAsync();
                break;
            case MenuOptions.Report:
                await _handlerFactory.CreatePerformanceReportHandler().HandleAsync();
                break;
            case MenuOptions.Delete:
                await _handlerFactory.CreateFileDeleteHandler().HandleAsync();
                break;
            case MenuOptions.Exit:
                return;
        }
    }

    private void WaitForUserInput()
    {
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[yellow]Press any key to continue...[/]");
        System.Console.ReadKey();
        AnsiConsole.Clear();
    }
    
}

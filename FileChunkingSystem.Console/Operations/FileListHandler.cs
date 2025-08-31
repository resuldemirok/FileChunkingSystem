using FileChunkingSystem.Application.Interfaces;
using FileChunkingSystem.Application.Models;
using FileChunkingSystem.Domain.Interfaces;
using FileChunkingSystem.Console.Handlers.Abstract;
using FileChunkingSystem.Domain.Entities;
using FileChunkingSystem.Console.Enums;
using Microsoft.Extensions.Logging;
using Spectre.Console.Rendering;
using Spectre.Console;
using Mapster;

namespace FileChunkingSystem.Console.Handlers;

public class FileListHandler : BaseHandler, IConsoleHandler
{
    private readonly IUnitOfWork _unitOfWork;

    public FileListHandler(
        IUnitOfWork unitOfWork,
        IFileChunkingService fileChunkingService,
        IPerformanceTrackingService performanceTrackingService,
        ILogger<FileListHandler> logger)
        : base(fileChunkingService, performanceTrackingService, logger)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task HandleAsync()
    {
        try
        {
            AnsiConsole.Write(new Rule("[bold blue]File List Management[/]"));
            
            var action = GetListAction();
            
            switch (action)
            {
                case ListAction.ViewAll:
                    await HandleViewAllFiles();
                    break;
                case ListAction.Search:
                    await HandleSearchFiles();
                    break;
                case ListAction.Filter:
                    await HandleFilterFiles();
                    break;
                case ListAction.Details:
                    await HandleFileDetails();
                    break;
                case ListAction.Export:
                    await HandleExportFileList();
                    break;
                case ListAction.Cleanup:
                    await HandleCleanupFiles();
                    break;
                default:
                    ShowWarning("Invalid action selected.");
                    break;
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error in file list operations");
            ShowError($"Operation failed: {ex.Message}");
            AnsiConsole.WriteException(ex);
            throw;
        }
    }

    private ListAction GetListAction()
    {
        var actions = new Dictionary<string, ListAction>
        {
            ["View All Files"] = ListAction.ViewAll,
            ["Search Files"] = ListAction.Search,
            ["Filter Files by Criteria"] = ListAction.Filter,
            ["View File Details"] = ListAction.Details,
            ["Export File List"] = ListAction.Export,
            ["Cleanup Old Files"] = ListAction.Cleanup
        };

        var choice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("What would you like to do?")
                .AddChoices(actions.Keys));

        return actions[choice];
    }

    private async Task HandleViewAllFiles()
    {
        ShowInfo("Loading all files...");
        
        var files = await _unitOfWork.Repository<FileMetadata>().GetAllAsync();
        
        if (!files.Any())
        {
            ShowWarning("No files found in the system.");
            return;
        }

        var model = files.Adapt<IEnumerable<FileMetadataModel>>();

        DisplayFileList(model, "All Files");
        
        // Offer additional actions
        await OfferFileActions(model);
    }

    private async Task HandleSearchFiles()
    {
        var searchTerm = AnsiConsole.Prompt(
            new TextPrompt<string>("Enter search term (file name, or type [red]q[/] to cancel):")
                .AllowEmpty()
        );

        if (string.Equals(searchTerm, "q", StringComparison.OrdinalIgnoreCase))
        {
            ShowInfo("Search cancelled.");
            return;
        }
        else if (string.IsNullOrWhiteSpace(searchTerm))
        {
            ShowWarning("Search term cannot be empty!");
            return;
        }

        ShowInfo($"Searching for files containing '{searchTerm}'...");
        
        var files = await FileChunkingService.SearchFilesAsync(searchTerm);
        
        if (!files.Any())
        {
            ShowWarning($"No files found matching '{searchTerm}'.");
            return;
        }

        DisplayFileList(files, $"Search Results for '{searchTerm}'");
        
        await OfferFileActions(files);
    }

    private async Task HandleFilterFiles()
    {
        var filter = BuildFileFilter();

        if (filter is null)
        {
            ShowInfo("Filter creation cancelled.");
            return;
        }
        
        ShowInfo("Applying filters...");
        
        var files = await FileChunkingService.GetFilteredFilesAsync(filter);
        
        if (!files.Any())
        {
            ShowWarning("No files found matching the specified criteria.");
            return;
        }

        DisplayFileList(files.Adapt<IEnumerable<FileMetadataModel>>(), "Filtered Results");
        
        await OfferFileActions(files);
    }

    private async Task HandleFileDetails()
    {
        var fileId = GetFileId("Enter file ID to view details:");

        if (fileId is null)
        {
            ShowInfo("Process cancelled.");
            return;
        }
        else if (fileId == Guid.Empty)
        {
            ShowWarning("Invalid input. Process cancelled.");
            return;
        }
        
        ShowInfo("Loading file details...");
        
        var fileDetails = await FileChunkingService.GetFileDetailsAsync(fileId.Value);
        
        if (fileDetails == null)
        {
            ShowError("File not found!");
            return;
        }

        DisplayFileDetails(fileDetails);
        
        await OfferSingleFileActions(fileDetails);
    }

    private async Task HandleExportFileList()
    {
        var files = await FileChunkingService.GetAllFileMetadataAsync();
        
        if (!files.Any())
        {
            ShowWarning("No files to export.");
            return;
        }

        var format = GetExportFormat();
        var includeChunkDetails = AnsiConsole.Confirm("Include chunk details in export?", false);
        
        await ExportFileList(files, format, includeChunkDetails);
    }

    private async Task HandleCleanupFiles()
    {
        var cleanupOptions = GetCleanupOptions();

        if (cleanupOptions is null)
        {
            ShowInfo("Cleanup cancelled.");
            return;
        }
        
        ShowInfo("Scanning for files to cleanup...");
        
        var filesToCleanup = await FileChunkingService.GetFilesForCleanupAsync(cleanupOptions);
        
        if (!filesToCleanup.Any())
        {
            ShowSuccess("No files found matching cleanup criteria.");
            return;
        }

        DisplayCleanupCandidates(filesToCleanup);
        
        if (AnsiConsole.Confirm($"Are you sure you want to delete {filesToCleanup.Count()} files?", false))
            await PerformCleanup(filesToCleanup);
        else
            ShowWarning("Cleanup cancelled.");
    }

    private FileFilterModel? BuildFileFilter()
    {
        var filter = new FileFilterModel();

        // File size filter
        var promptResult = PromptYesNo("Filter by file size?");
        if (promptResult is null) return null;
        if (promptResult == true)
        {
            var minSize = GetOptionalLong("Minimum file size (bytes, leave empty for no limit, or type q to cancel):");
            if (minSize is null) return null;
            filter.MinSize = minSize.Value;

            var maxSize = GetOptionalLong("Maximum file size (bytes, leave empty for no limit, or type q to cancel):");
            if (maxSize is null) return null;
            filter.MaxSize = maxSize.Value;
        }

        // Date range filter
        promptResult = PromptYesNo("Filter by upload date?");
        if (promptResult is null) return null;
        if (promptResult == true)
        {
            var startDate = GetOptionalDateTime("Start date (yyyy-MM-dd, leave empty for no limit, or type q to cancel):");
            if (startDate is null) return null;
            filter.StartDate = startDate;

            var endDate = GetOptionalDateTime("End date (yyyy-MM-dd, leave empty for no limit, or type q to cancel):");
            if (endDate is null) return null;
            filter.EndDate = endDate;
        }

        // File extension filter
        promptResult = PromptYesNo("Filter by file extension?");
        if (promptResult is null) return null;
        if (promptResult == true)
        {
            var extension = GetOptionalString("Enter file extension (without dot, e.g., 'pdf', 'txt', or type q to cancel):");
            if (extension is null) return null;
            else if (extension != string.Empty)
                filter.FileExtension = extension.StartsWith('.') ? extension : $".{extension}";
        }

        // Chunk count filter
        promptResult = PromptYesNo("Filter by chunk count?");
        if (promptResult is null) return null;
        if (promptResult == true)
        {
            var minChunk = GetOptionalInt("Minimum chunk count (leave empty for no limit, or type q to cancel):");
            if (minChunk is null) return null;
            filter.MinChunkCount = minChunk;

            var maxChunk = GetOptionalInt("Maximum chunk count (leave empty for no limit, or type q to cancel):");
            if (maxChunk is null) return null;
            filter.MaxChunkCount = maxChunk;
        }

        return filter;
    }

    private void DisplayFileList(IEnumerable<FileMetadataModel> files, string title)
    {
        var table = new Table();
        table.Title = new TableTitle(title);
        table.Border = TableBorder.Rounded;
        
        table.AddColumn("ID");
        table.AddColumn("File Name");
        table.AddColumn("Size");
        table.AddColumn("Chunks");
        table.AddColumn("Strategy");
        table.AddColumn("Uploaded");

        foreach (var file in files.OrderByDescending(f => f.CreatedAt))
        {
            var fileName = file.OriginalFileName.Length > 30 
                ? file.OriginalFileName[..27] + "..." 
                : file.OriginalFileName;

            table.AddRow(
                file.Id.ToString()[..8] + "...",
                fileName,
                FormatBytes(file.FileSize),
                file.TotalChunks.ToString("N0"),
                file.ChunkingAlgorithm.ToString(),
                file.CreatedAt.ToString("yyyy-MM-dd HH:mm")
            );
        }

        AnsiConsole.Write(table);
        
        // Display summary
        DisplayFileListSummary(files);
    }

    private void DisplayFileListSummary(IEnumerable<FileMetadataModel> files)
    {
        var filesList = files.ToList();
        var totalFiles = filesList.Count;
        var totalSize = filesList.Sum(f => f.FileSize);
        var totalChunks = filesList.Sum(f => f.TotalChunks);
        var avgChunksPerFile = totalFiles > 0 ? (double)totalChunks / totalFiles : 0;

        var summaryTable = new Table();
        summaryTable.Border = TableBorder.Simple;
        summaryTable.AddColumn("Metric");
        summaryTable.AddColumn("Value");

        summaryTable.AddRow("Total Files", $"{totalFiles:N0}");
        summaryTable.AddRow("Total Size", FormatBytes(totalSize));
        summaryTable.AddRow("Total Chunks", $"{totalChunks:N0}");
        summaryTable.AddRow("Avg Chunks/File", $"{avgChunksPerFile:F1}");

        var panel = new Panel(summaryTable)
            .Header("Summary")
            .Border(BoxBorder.Rounded)
            .BorderColor(Color.Blue);

        AnsiConsole.Write(panel);
    }

    private void DisplayFileDetails(FileMetadataModel fileDetails)
    {
        var panel = new Panel(CreateFileDetailsContent(fileDetails))
            .Header($"File Details - {fileDetails.OriginalFileName}")
            .Border(BoxBorder.Rounded)
            .BorderColor(Color.Green);

        AnsiConsole.Write(panel);

        // Display chunk information
        if (fileDetails.Chunks?.Any() == true)
            DisplayChunkDetails(fileDetails.Chunks);
    }

    private IRenderable CreateFileDetailsContent(FileMetadataModel fileDetails)
    {
        var grid = new Grid()
            .AddColumn()
            .AddColumn();

        grid.AddRow("File ID:", fileDetails.Id.ToString());
        grid.AddRow("File Name:", fileDetails.OriginalFileName);
        grid.AddRow("File Size:", FormatBytes(fileDetails.FileSize));
        grid.AddRow("MIME Type:", fileDetails.FileExtension ?? "Unknown");
        grid.AddRow("Chunk Count:", fileDetails.TotalChunks.ToString("N0"));
        grid.AddRow("Chunking Strategy:", fileDetails.ChunkingAlgorithm.ToString());
        grid.AddRow("Uploaded At:", fileDetails.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"));

        return grid;
    }

    private void DisplayChunkDetails(IEnumerable<ChunkMetadataModel> chunks)
    {
        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Rule("[bold yellow]Chunk Details[/]"));

        var table = new Table();
        table.AddColumn("Chunk #");
        table.AddColumn("Size");
        table.AddColumn("Hash");
        table.AddColumn("Storage Type");
        table.AddColumn("Created At");

        foreach (var chunk in chunks.OrderBy(c => c.ChunkIndex))
        {
            var hash = chunk.ChecksumSha256?.Length > 16 
                ? chunk.ChecksumSha256[..16] + "..." 
                : chunk.ChecksumSha256 ?? "N/A";

            table.AddRow(
                chunk.ChunkIndex.ToString(),
                FormatBytes(chunk.ChunkSize),
                hash,
                chunk.StorageProviderType.ToString(),
                chunk.CreatedAt.ToString("yyyy-MM-dd HH:mm")
            );
        }

        AnsiConsole.Write(table);
    }

    private void DisplayCleanupCandidates(IEnumerable<FileMetadataModel> files)
    {
        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Rule("[bold red]Files to be Deleted[/]"));

        DisplayFileList(files, "Cleanup Candidates");
        
        var totalSize = files.Sum(f => f.FileSize);
        ShowWarning($"Total size to be freed: {FormatBytes(totalSize)}");
    }

    private Dictionary<string, MenuFileActions> CreateMenuFileActions()
    {
        return new Dictionary<string, MenuFileActions>
        {
            ["View Details"] = MenuFileActions.ViewDetails,
            ["Download/Merge"] = MenuFileActions.DownloadMerge,
            ["Delete"] = MenuFileActions.Delete,
            ["Update Metadata"] = MenuFileActions.UpdateMetadata
        };
    }

    private async Task OfferFileActions(IEnumerable<FileMetadataModel> files)
    {
        if (!AnsiConsole.Confirm("Would you like to perform an action on any of these files?", false))
            return;

        var menuFileActions = CreateMenuFileActions();

        var action = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Select action:")
                .AddChoices(menuFileActions.Keys));

        var fileId = GetFileId("Enter file ID:");

        if (fileId is null)
        {
            ShowInfo("Process cancelled.");
            return;
        }
        else if (fileId == Guid.Empty)
        {
            ShowWarning("Invalid input. Process cancelled.");
            return;
        }

        switch (menuFileActions[action])
        {
            case MenuFileActions.ViewDetails:
                var details = await FileChunkingService.GetFileDetailsAsync(fileId.Value);
                if (details != null)
                    DisplayFileDetails(details);
                else
                    ShowError("File not found!");
                break;
                
            case MenuFileActions.DownloadMerge:
                await OfferFileMerge(fileId.Value);
                break;
                
            case MenuFileActions.Delete:
                await OfferFileDelete(fileId.Value);
                break;
                
            case MenuFileActions.UpdateMetadata:
                OfferMetadataUpdate(fileId.Value);
                break;
        }
    }

    private Dictionary<string, MenuSingleFileActions> CreateMenuSingleFileActions()
    {
        return new Dictionary<string, MenuSingleFileActions>
        {
            ["Download/Merge"] = MenuSingleFileActions.DownloadMerge,
            ["Delete"] = MenuSingleFileActions.Delete,
            ["Update Metadata"] = MenuSingleFileActions.UpdateMetadata,
            ["Verify Integrity"] = MenuSingleFileActions.VerifyIntegrity
        };
    }

    private async Task OfferSingleFileActions(FileMetadataModel fileDetails)
    {
        if (!AnsiConsole.Confirm("Would you like to perform an action on this file?", false))
            return;

        var menuFileActions = CreateMenuSingleFileActions();

        var action = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Select action:")
                .AddChoices(menuFileActions.Keys));

        switch (menuFileActions[action])
        {
            case MenuSingleFileActions.DownloadMerge:
                await OfferFileMerge(fileDetails.Id);
                break;
                
            case MenuSingleFileActions.Delete:
                await OfferFileDelete(fileDetails.Id);
                break;
                
            case MenuSingleFileActions.UpdateMetadata:
                OfferMetadataUpdate(fileDetails.Id);
                break;
                
            case MenuSingleFileActions.VerifyIntegrity:
                await OfferIntegrityCheck(fileDetails.Id);
                break;
        }
    }

    private async Task OfferFileMerge(Guid fileId)
    {
        var outputPath = AnsiConsole.Ask<string>("Enter output path for merged file:");
        var overwrite = !File.Exists(outputPath) || AnsiConsole.Confirm("This file is exists. Do you want to overwrite it?", false);

        if (!overwrite) {
            ShowInfo("Merge cancelled.");
            return;
        }

        try
        {
            var mergeModel = new FileMergeModel
            {
                FileMetadataId = fileId,
                OutputPath = Path.Combine("restored", outputPath),
                OverwriteIfExists = overwrite
            };

            if (!Directory.Exists("restored"))
                Directory.CreateDirectory("restored");

            var progress = new Progress<ProgressModel>(p =>
            {
                AnsiConsole.MarkupLine($"[green]{p.Operation}[/] - {p.PercentageComplete:F1}%");
            });

            var mergedData = await FileChunkingService.MergeAndRetrieveFileAsync(mergeModel, progress);
            await File.WriteAllBytesAsync(outputPath, mergedData);
            
            ShowSuccess($"File merged and saved to: {outputPath}");
        }
        catch (Exception ex)
        {
            ShowError($"Merge failed: {ex.Message}");
        }
    }

    private async Task OfferFileDelete(Guid fileId)
    {
        if (!AnsiConsole.Confirm("Are you sure you want to delete this file?", false))
        {
            ShowWarning("Delete cancelled.");
            return;
        }

        try
        {
            var deleted = await FileChunkingService.DeleteFileAsync(fileId);
            if (deleted)
                ShowSuccess("File deleted successfully!");
            else
                ShowError("File not found or could not be deleted!");
        }
        catch (Exception ex)
        {
            ShowError($"Delete failed: {ex.Message}");
        }
    }

    private void OfferMetadataUpdate(Guid fileId)
    {
        var description = AnsiConsole.Ask<string>("Enter new description (leave empty to skip):");
        
        var customMetadata = new Dictionary<string, string>();
        
        while (AnsiConsole.Confirm("Add custom metadata?", false))
        {
            var key = AnsiConsole.Ask<string>("Metadata key:");
            var value = AnsiConsole.Ask<string>("Metadata value:");
            customMetadata[key] = value;
        }

        try
        {
            ShowSuccess("Metadata updated successfully!");
        }
        catch (Exception ex)
        {
            ShowError($"Metadata update failed: {ex.Message}");
        }
    }

    private async Task OfferIntegrityCheck(Guid fileId)
    {
        ShowInfo("Performing integrity check...");
        
        try
        {
            var result = await FileChunkingService.VerifyFileIntegrityAsync(fileId);
            
            if (result.IsValid)
            {
                ShowSuccess("File integrity check passed!");
                ShowInfo($"All {result.TotalChunks} chunks verified successfully.");
            }
            else
            {
                ShowError("File integrity check failed!");
                ShowWarning($"Found {result.CorruptedChunks.Count} corrupted chunks out of {result.TotalChunks}");
                
                if (result.CorruptedChunks.Any())
                {
                    ShowInfo("Corrupted chunk indices: " + string.Join(", ", result.CorruptedChunks));
                }
            }
        }
        catch (Exception ex)
        {
            ShowError($"Integrity check failed: {ex.Message}");
        }
    }

    private CleanupOptionsModel? GetCleanupOptions()
    {
        var options = new CleanupOptionsModel();

        var optionalInt = GetOptionalInt("Delete files older than X days (leave empty for no age limit):");
        if (optionalInt is null) return null;
        options.DeleteOlderThanDays = optionalInt.Value == 0 ? null : optionalInt.Value;
        
        var optionalLong = GetOptionalLong("Delete files larger than X bytes (leave empty for no size limit):");
        if (optionalLong is null) return null;
        options.DeleteLargerThanBytes = optionalLong.Value == 0 ? null : optionalLong.Value;

        optionalInt = GetOptionalInt("Delete files not accessed for X days (leave empty to skip):");
        if (optionalInt is null) return null;
        options.DeleteUnusedForDays = optionalInt.Value == 0 ? null : optionalInt.Value;

        options.DryRun = AnsiConsole.Confirm("Perform dry run first?", true);

        return options;
    }

    private async Task PerformCleanup(IEnumerable<FileMetadataModel> filesToCleanup)
    {
        var filesList = filesToCleanup.ToList();
        var deletedCount = 0;
        var totalSize = 0L;

        await AnsiConsole.Progress()
            .StartAsync(async ctx =>
            {
                var task = ctx.AddTask("[red]Deleting files...[/]", maxValue: filesList.Count);

                foreach (var file in filesList)
                {
                    try
                    {
                        var deleted = await FileChunkingService.DeleteFileAsync(file.Id);
                        if (deleted)
                        {
                            deletedCount++;
                            totalSize += file.FileSize;
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.LogWarning(ex, "Failed to delete file {FileId}", file.Id);
                    }

                    task.Increment(1);
                }
            });

        ShowSuccess($"Cleanup completed! Deleted {deletedCount} files, freed {FormatBytes(totalSize)}");
    }

    private async Task ExportFileList(IEnumerable<FileMetadataModel> files, MenuExportOptions format, bool includeChunkDetails)
    {
        var fileName = $"file_list_{DateTime.Now:yyyyMMdd_HHmmss}.{format.ToString().ToLower()}";
        var filePath = Path.Combine(Environment.CurrentDirectory, fileName);

        try
        {
            switch (format)
            {
                case MenuExportOptions.CSV:
                    await ExportToCsv(files, filePath, includeChunkDetails);
                    break;
                case MenuExportOptions.JSON:
                    await ExportToJson(files, filePath, includeChunkDetails);
                    break;
                case MenuExportOptions.XML:
                    await ExportToXml(files, filePath, includeChunkDetails);
                    break;
            }

            ShowSuccess($"File list exported to: {filePath}");
            
            if (AnsiConsole.Confirm("Open the exported file?", false))
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = filePath,
                    UseShellExecute = true
                });
            }
        }
        catch (Exception ex)
        {
            ShowError($"Export failed: {ex.Message}");
        }
    }

    private async Task ExportToCsv(IEnumerable<FileMetadataModel> files, string filePath, bool includeChunkDetails)
    {
        var csv = new System.Text.StringBuilder();
        
        // Header
        csv.AppendLine("ID,FileName,FileSize,ChunkCount,ChunkingStrategy,UploadedAt,MimeType");
        
        foreach (var file in files)
        {
            csv.AppendLine($"\"{file.Id}\",\"{file.OriginalFileName}\",{file.FileSize},{file.TotalChunks}," +
                          $"\"{file.ChunkingAlgorithm.ToString()}\",\"{file.CreatedAt:yyyy-MM-dd HH:mm:ss}\"," +
                          $"\"{file.FileExtension}\"");
        }

        await File.WriteAllTextAsync(filePath, csv.ToString());
    }

    private async Task ExportToJson(IEnumerable<FileMetadataModel> files, string filePath, bool includeChunkDetails)
    {
        var options = new System.Text.Json.JsonSerializerOptions 
        { 
            WriteIndented = true,
            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
        };
        
        var json = System.Text.Json.JsonSerializer.Serialize(files, options);
        await File.WriteAllTextAsync(filePath, json);
    }

    private async Task ExportToXml(IEnumerable<FileMetadataModel> files, string filePath, bool includeChunkDetails)
    {
        var xml = new System.Text.StringBuilder();
        xml.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
        xml.AppendLine("<FileList>");
        
        foreach (var file in files)
        {
            xml.AppendLine("  <File>");
            xml.AppendLine($"    <Id>{file.Id}</Id>");
            xml.AppendLine($"    <FileName><![CDATA[{file.OriginalFileName}]]></FileName>");
            xml.AppendLine($"    <FileSize>{file.FileSize}</FileSize>");
            xml.AppendLine($"    <ChunkCount>{file.TotalChunks}</ChunkCount>");
            xml.AppendLine($"    <ChunkingStrategy><![CDATA[{file.ChunkingAlgorithm.ToString()}]]></ChunkingStrategy>");
            xml.AppendLine($"    <UploadedAt>{file.CreatedAt:yyyy-MM-dd HH:mm:ss}</UploadedAt>");
            xml.AppendLine($"    <MimeType><![CDATA[{file.FileExtension}]]></MimeType>");
            xml.AppendLine("  </File>");
        }
        
        xml.AppendLine("</FileList>");
        await File.WriteAllTextAsync(filePath, xml.ToString());
    }

    private Guid? GetFileId(string prompt)
    {
        var fileIdInput = AnsiConsole.Prompt(
            new TextPrompt<string>($"{prompt} (or type [red]q[/] to cancel):")
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

    private Dictionary<string, MenuExportOptions> CreateMenuExportOptions()
    {
        return new Dictionary<string, MenuExportOptions>
        {
            ["CSV"] = MenuExportOptions.CSV,
            ["JSON"] = MenuExportOptions.JSON,
            ["XML"] = MenuExportOptions.XML
        };
    }

    private MenuExportOptions GetExportFormat()
    {
        var menuExportOptions = CreateMenuExportOptions();

        var format = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Select export format:")
                .AddChoices(menuExportOptions.Keys));

        return menuExportOptions[format];
    }

    private long? GetOptionalLong(string prompt)
    {
        while (true)
        {  
            var input = AnsiConsole.Prompt(
                new TextPrompt<string>(prompt + " (or type [red]q[/] to cancel):")
                    .AllowEmpty()
            );

            if (string.Equals(input, "q", StringComparison.OrdinalIgnoreCase))
                return null;

            if (string.IsNullOrWhiteSpace(input))
                return -1;

            if (long.TryParse(input, out var value) && value >= 0)
                return value;

            AnsiConsole.MarkupLine("[red]Invalid number! Please enter a valid long or 'q' to cancel.[/]");
        }
    }

    private string? GetOptionalString(string prompt)
    {
        var input = AnsiConsole.Ask<string>(prompt);

        if (string.Equals(input, "q", StringComparison.OrdinalIgnoreCase))
            return null;

        if (string.IsNullOrWhiteSpace(input))
            return "";

        return input;
    }

    private int? GetOptionalInt(string prompt)
    {
        while (true)
        {
            var input = AnsiConsole.Prompt(
                new TextPrompt<string>(prompt + " (or type [red]q[/] to cancel):")
                    .AllowEmpty()
            );

            if (string.Equals(input, "q", StringComparison.OrdinalIgnoreCase))
                return null;

            if (string.IsNullOrWhiteSpace(input))
                return -1;

            if (int.TryParse(input, out var value) && value >= 0)
                return value;

            AnsiConsole.MarkupLine("[red]Invalid number! Please enter a valid integer or 'q' to cancel.[/]");
        }
    }

    private DateTime? GetOptionalDateTime(string prompt)
    {
        var input = AnsiConsole.Ask<string>(prompt);
        return string.IsNullOrWhiteSpace(input) ? null : DateTime.Parse(input);
    }

}

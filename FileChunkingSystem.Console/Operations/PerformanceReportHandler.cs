using FileChunkingSystem.Application.Interfaces;
using FileChunkingSystem.Application.Models;
using FileChunkingSystem.Console.Handlers.Abstract;
using FileChunkingSystem.Console.Enums;
using Microsoft.Extensions.Logging;
using Spectre.Console.Rendering;
using System.Text.Json;
using Spectre.Console;
using System.Text;

namespace FileChunkingSystem.Console.Handlers;

public class PerformanceReportHandler : BaseHandler, IConsoleHandler
{
    public PerformanceReportHandler(
        IFileChunkingService fileChunkingService,
        IPerformanceTrackingService performanceTrackingService,
        ILogger<PerformanceReportHandler> logger)
        : base(fileChunkingService, performanceTrackingService, logger)
    {
    }

    public async Task HandleAsync()
    {
        try
        {
            AnsiConsole.Write(new Rule("[bold blue]Performance Report Generation[/]"));
            
            var (startDate, endDate, ok) = GetDateRange();

            if (!ok)
            {
                ShowWarning("Report generation cancelled.");
                return;
            }

            var report = await GenerateReport(startDate, endDate);
            
            DisplayReport(report);
            
            if (AnsiConsole.Confirm("Do you want to save this report to a file?", false))
            {
                await ExportReport(report);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error generating performance report");
            ShowError($"Report generation failed: {ex.Message}");
            AnsiConsole.WriteException(ex);
            throw;
        }
    }

    private (DateTime? startDate, DateTime? endDate, bool ok) GetDateRange()
    {
        var useDateRange = PromptYesNo("Do you want to specify a date range?");
        
        if (useDateRange is null)
            return (null, null, false);
        else if (useDateRange == false)
            return (null, null, true);

        var startDate = GetStartDate();
        if (startDate is null)
            return (null, null, false);

        var endDate = GetEndDate();
        if (endDate is null)
            return (null, null, false);

        return (startDate, endDate, true);
    }

    private DateTime? GetStartDate()
    {
        while (true)
        {
            var startDateInput = AnsiConsole.Prompt(
                new TextPrompt<string>("Enter start date (yyyy-MM-dd) or press Enter for 30 days ago (or type q to cancel):")
                    .AllowEmpty()
            );

            if (string.Equals(startDateInput, "q", StringComparison.OrdinalIgnoreCase))
                return null;

            if (string.IsNullOrWhiteSpace(startDateInput))
                return DateTime.Now.AddDays(-30);

            if (DateTime.TryParse(startDateInput, out var parsedStartDate))
                return parsedStartDate;

            ShowWarning("Invalid date format, please try again or press Enter for default (30 days ago).");
        }
    }

    private DateTime? GetEndDate()
    {
        while (true)
        {
            var endDateInput = AnsiConsole.Prompt(
                new TextPrompt<string>("Enter end date (yyyy-MM-dd) or press Enter for today (or type q to cancel):")
                    .AllowEmpty()
            );

            if (string.Equals(endDateInput, "q", StringComparison.OrdinalIgnoreCase))
                return null;

            if (string.IsNullOrWhiteSpace(endDateInput))
                return DateTime.Now;

            if (DateTime.TryParse(endDateInput, out var parsedEndDate))
                return parsedEndDate;

            ShowWarning("Invalid date format, please try again or press Enter for default (today).");
        }
    }

    private async Task<PerformanceReportModel> GenerateReport(DateTime? startDate, DateTime? endDate)
    {
        return await AnsiConsole.Progress()
            .StartAsync(async ctx =>
            {
                var task = ctx.AddTask("[green]Generating performance report...[/]");
                task.MaxValue = 100;

                task.Value = 25;
                await Task.Delay(100); // Simulate work

                task.Value = 50;
                var result = await PerformanceTrackingService.GeneratePerformanceReportAsync(startDate, endDate);
                
                task.Value = 75;
                await Task.Delay(100); // Simulate processing

                task.Value = 100;
                return result;
            });
    }

    private void DisplayReport(PerformanceReportModel report)
    {
        var panel = new Panel(CreateReportContent(report))
            .Header("Performance Report")
            .Border(BoxBorder.Rounded)
            .BorderColor(Color.Green);

        AnsiConsole.Write(panel);

        // Display additional details if available
        DisplayDetailedStats(report);
    }

    private IRenderable CreateReportContent(PerformanceReportModel report)
    {
        var grid = new Grid()
            .AddColumn()
            .AddColumn();

        grid.AddRow("Report Period:", $"{report.ReportPeriod.StartDate:yyyy-MM-dd} to {report.ReportPeriod.EndDate:yyyy-MM-dd}");
        grid.AddRow("Total Files Processed:", $"{report.TotalFilesProcessed:N0}");
        grid.AddRow("Total Bytes Processed:", FormatBytes(report.TotalBytesProcessed));
        grid.AddRow("Total Chunks Created:", $"{report.TotalChunksCreated:N0}");
        grid.AddRow("Average Chunking Time:", $"{report.AverageChunkingTime:F2} ms");
        grid.AddRow("Average Merging Time:", $"{report.AverageMergingTime:F2} ms");

        if (report.ErrorStats?.TotalErrors > 0)
        {
            grid.AddRow("[red]Total Errors:[/]", $"[red]{report.ErrorStats.TotalErrors:N0}[/]");
        }

        return grid;
    }

    private void DisplayDetailedStats(PerformanceReportModel report)
    {
        // File Size Distribution
        if (report.FileSizeDistribution?.Any() == true)
        {
            DisplayFileSizeDistribution(report.FileSizeDistribution);
        }

        // Chunking Strategy Stats
        if (report.ChunkingStrategyStats?.Any() == true)
        {
            DisplayChunkingStrategyStats(report.ChunkingStrategyStats);
        }

        // Storage Type Stats
        if (report.StorageTypeStats?.Any() == true)
        {
            DisplayStorageTypeStats(report.StorageTypeStats);
        }
    }

    private void DisplayFileSizeDistribution(Dictionary<string, int> distribution)
    {
        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Rule("[bold yellow]File Size Distribution[/]"));

        var table = new Table();
        table.AddColumn("Size Range");
        table.AddColumn("File Count");
        table.AddColumn("Percentage");

        var total = distribution.Values.Sum();

        foreach (var item in distribution)
        {
            var percentage = total > 0 ? (double)item.Value / total * 100 : 0;
            table.AddRow(
                item.Key,
                $"{item.Value:N0}",
                $"{percentage:F1}%"
            );
        }

        AnsiConsole.Write(table);
    }

    private void DisplayChunkingStrategyStats(Dictionary<string, ChunkingStrategyStats> strategyStats)
    {
        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Rule("[bold yellow]Chunking Strategy Statistics[/]"));

        var table = new Table();
        table.AddColumn("Strategy");
        table.AddColumn("File Count");
        table.AddColumn("Total Bytes");
        table.AddColumn("Avg Chunks");
        table.AddColumn("Avg Chunk Size");

        foreach (var strategy in strategyStats)
        {
            table.AddRow(
                strategy.Key,
                $"{strategy.Value.FileCount:N0}",
                FormatBytes(strategy.Value.TotalBytes),
                $"{strategy.Value.AverageChunks:F1}",
                FormatBytes((long)strategy.Value.AverageChunkSize)
            );
        }

        AnsiConsole.Write(table);
    }

    private void DisplayStorageTypeStats(Dictionary<string, StorageTypeStats> storageStats)
    {
        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Rule("[bold yellow]Storage Type Statistics[/]"));

        var table = new Table();
        table.AddColumn("Storage Type");
        table.AddColumn("Chunk Count");
        table.AddColumn("Total Bytes");
        table.AddColumn("Avg Response Time");
        table.AddColumn("Error Count");

        foreach (var storage in storageStats)
        {
            var errorColor = storage.Value.ErrorCount > 0 ? "[red]" : "[green]";
            table.AddRow(
                storage.Key,
                $"{storage.Value.ChunkCount:N0}",
                FormatBytes(storage.Value.TotalBytes),
                $"{storage.Value.AverageResponseTime:F2} ms",
                $"{errorColor}{storage.Value.ErrorCount:N0}[/]"
            );
        }

        AnsiConsole.Write(table);
    }

    private void DisplayTopLargestFiles(List<TopFileInfo> topFiles)
    {
        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Rule("[bold yellow]Top Largest Files[/]"));

        var table = new Table();
        table.AddColumn("Rank");
        table.AddColumn("File Name");
        table.AddColumn("File Size");
        table.AddColumn("Chunk Count");
        table.AddColumn("Strategy");
        table.AddColumn("Processed At");

        var counter = 1;
        foreach (var file in topFiles.Take(10)) // Show top 10
        {
            var fileName = file.FileName.Length > 30 
                ? file.FileName[..27] + "..." 
                : file.FileName;

            table.AddRow(
                counter.ToString(),
                fileName,
                FormatBytes(file.FileSize),
                $"{file.ChunkCount:N0}",
                file.ChunkingStrategy ?? "Unknown",
                file.ProcessedAt.ToString("yyyy-MM-dd HH:mm")
            );
            counter++;
        }

        AnsiConsole.Write(table);
    }

    private void DisplayErrorStats(ErrorStatistics errorStats)
    {
        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Rule("[bold red]Error Statistics[/]"));

        AnsiConsole.MarkupLine($"[red]Total Errors: {errorStats.TotalErrors:N0}[/]");

        if (errorStats.ErrorsByType?.Any() == true)
        {
            var table = new Table();
            table.AddColumn("Error Type");
            table.AddColumn("Count");
            table.AddColumn("Percentage");

            foreach (var error in errorStats.ErrorsByType.OrderByDescending(e => e.Value))
            {
                var percentage = (double)error.Value / errorStats.TotalErrors * 100;
                table.AddRow(
                    error.Key,
                    $"[red]{error.Value:N0}[/]",
                    $"{percentage:F1}%"
                );
            }

            AnsiConsole.Write(table);
        }
    }

    private async Task ExportReport(PerformanceReportModel report)
    {
        try
        {
            var format = GetExportFormat();
            var fileName = GenerateFileName(format.ToString());
            var filePath = Path.Combine(Environment.CurrentDirectory, fileName);

            await ExportInFormat(report, filePath, format);

            ShowSuccess($"Report exported to: {filePath}");
            
            if (AnsiConsole.Confirm("Do you want to open the file?", false))
                OpenFile(filePath);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to export performance report");
            ShowError($"Export failed: {ex.Message}");
        }
    }

    private MenuPerformanceExportOptions GetExportFormat()
    {
        var menuExportOptions = CreateMenuExportOptions();

        var format = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Select export format:")
                .AddChoices(menuExportOptions.Keys));

        return menuExportOptions[format];
    }

    private string GenerateFileName(string format)
    {
        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var extension = format.ToLower();
        return $"performance_report_{timestamp}.{extension}";
    }

    private Dictionary<string, MenuPerformanceExportOptions> CreateMenuExportOptions()
    {
        return new Dictionary<string, MenuPerformanceExportOptions>
        {
            ["JSON"] = MenuPerformanceExportOptions.JSON,
            ["CSV"] = MenuPerformanceExportOptions.CSV,
            ["HTML"] = MenuPerformanceExportOptions.HTML,
            ["TXT"] = MenuPerformanceExportOptions.TXT
        };
    }

    private async Task ExportInFormat(PerformanceReportModel report, string filePath, MenuPerformanceExportOptions format)
    {
        switch (format)
        {
            case MenuPerformanceExportOptions.JSON:
                await ExportAsJson(report, filePath);
                break;
            case MenuPerformanceExportOptions.CSV:
                await ExportAsCsv(report, filePath);
                break;
            case MenuPerformanceExportOptions.HTML:
                await ExportAsHtml(report, filePath);
                break;
            case MenuPerformanceExportOptions.TXT:
                await ExportAsText(report, filePath);
                break;
        }
    }

    private async Task ExportAsJson(PerformanceReportModel report, string filePath)
    {
        var options = new JsonSerializerOptions 
        { 
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        
        var json = JsonSerializer.Serialize(report, options);
        await File.WriteAllTextAsync(filePath, json);
    }

    private async Task ExportAsCsv(PerformanceReportModel report, string filePath)
    {
        var csv = new StringBuilder();
        
        // Header
        csv.AppendLine("Performance Report CSV Export");
        csv.AppendLine($"Generated,{DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        csv.AppendLine($"Report Period,{report.ReportPeriod.StartDate:yyyy-MM-dd} to {report.ReportPeriod.EndDate:yyyy-MM-dd}");
        csv.AppendLine();
        
        // General Statistics
        csv.AppendLine("General Statistics");
        csv.AppendLine("Metric,Value");
        csv.AppendLine($"Total Files Processed,{report.TotalFilesProcessed}");
        csv.AppendLine($"Total Bytes Processed,{report.TotalBytesProcessed}");
        csv.AppendLine($"Total Chunks Created,{report.TotalChunksCreated}");
        csv.AppendLine($"Average Chunking Time (ms),{report.AverageChunkingTime:F2}");
        csv.AppendLine($"Average Merging Time (ms),{report.AverageMergingTime:F2}");
        csv.AppendLine();
        
        // Add other sections...
        AddFileSizeDistributionToCsv(csv, report.FileSizeDistribution);
        AddChunkingStrategyStatsToCsv(csv, report.ChunkingStrategyStats);
        AddStorageTypeStatsToCsv(csv, report.StorageTypeStats);
        
        await File.WriteAllTextAsync(filePath, csv.ToString());
    }

    private async Task ExportAsHtml(PerformanceReportModel report, string filePath)
    {
        var html = new StringBuilder();
        
        html.AppendLine("<!DOCTYPE html>");
        html.AppendLine("<html>");
        html.AppendLine("<head>");
        html.AppendLine("    <meta charset='utf-8'>");
        html.AppendLine("    <title>Performance Report</title>");
        html.AppendLine("    <style>");
        html.AppendLine(GetHtmlStyles());
        html.AppendLine("    </style>");
        html.AppendLine("</head>");
        html.AppendLine("<body>");
        
        // Add HTML content...
        AddHtmlHeader(html, report);
        AddHtmlGeneralStats(html, report);
        AddHtmlDetailedStats(html, report);
        
        html.AppendLine("</body>");
        html.AppendLine("</html>");
        
        await File.WriteAllTextAsync(filePath, html.ToString());
    }

    private async Task ExportAsText(PerformanceReportModel report, string filePath)
    {
        var text = new StringBuilder();
        
        text.AppendLine("FILE CHUNKING SYSTEM - PERFORMANCE REPORT");
        text.AppendLine("=========================================");
        text.AppendLine();
        text.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        text.AppendLine($"Report Period: {report.ReportPeriod.StartDate:yyyy-MM-dd} to {report.ReportPeriod.EndDate:yyyy-MM-dd}");
        text.AppendLine();
        
        // Add text content...
        AddTextGeneralStats(text, report);
        AddTextDetailedStats(text, report);
        
        await File.WriteAllTextAsync(filePath, text.ToString());
    }

    private void OpenFile(string filePath)
    {
        try
        {
            var processInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = filePath,
                UseShellExecute = true
            };
            
            System.Diagnostics.Process.Start(processInfo);
        }
        catch (Exception ex)
        {
            ShowWarning($"Could not open file: {ex.Message}");
        }
    }

    // Helper methods for different export formats
    private void AddFileSizeDistributionToCsv(StringBuilder csv, Dictionary<string, int>? distribution)
    {
        if (distribution?.Any() != true) return;

        csv.AppendLine("File Size Distribution");
        csv.AppendLine("Size Range,File Count");
        
        foreach (var item in distribution)
            csv.AppendLine($"{item.Key},{item.Value}");
        
        csv.AppendLine();
    }

    private void AddChunkingStrategyStatsToCsv(StringBuilder csv, Dictionary<string, ChunkingStrategyStats>? strategyStats)
    {
        if (strategyStats?.Any() != true) return;

        csv.AppendLine("Chunking Strategy Statistics");
        csv.AppendLine("Strategy,File Count,Total Bytes,Average Chunks,Average Chunk Size");
        
        foreach (var strategy in strategyStats)
            csv.AppendLine($"{strategy.Key},{strategy.Value.FileCount},{strategy.Value.TotalBytes},{strategy.Value.AverageChunks:F1},{strategy.Value.AverageChunkSize:F0}");
        
        csv.AppendLine();
    }

    private void AddStorageTypeStatsToCsv(StringBuilder csv, Dictionary<string, StorageTypeStats>? storageStats)
    {
        if (storageStats?.Any() != true) return;

        csv.AppendLine("Storage Type Statistics");
        csv.AppendLine("Storage Type,Chunk Count,Total Bytes,Average Response Time (ms),Error Count");
        
        foreach (var storage in storageStats)
            csv.AppendLine($"{storage.Key},{storage.Value.ChunkCount},{storage.Value.TotalBytes},{storage.Value.AverageResponseTime:F2},{storage.Value.ErrorCount}");
        
        csv.AppendLine();
    }

    private void AddTopLargestFilesToCsv(StringBuilder csv, List<TopFileInfo>? topFiles)
    {
        if (topFiles?.Any() != true) return;

        csv.AppendLine("Top Largest Files");
        csv.AppendLine("File Name,File Size,Chunk Count,Chunking Strategy,Processed At");
        foreach (var file in topFiles)
        {
            csv.AppendLine($"\"{file.FileName}\",{file.FileSize},{file.ChunkCount},{file.ChunkingStrategy},{file.ProcessedAt:yyyy-MM-dd HH:mm:ss}");
        }
    }

    private string GetHtmlStyles()
    {
        return @"
        body { font-family: Arial, sans-serif; margin: 20px; }
        .header { background-color: #f0f0f0; padding: 20px; border-radius: 5px; margin-bottom: 20px; }
        .section { margin-bottom: 30px; }
        .section h2 { color: #333; border-bottom: 2px solid #007acc; padding-bottom: 5px; }
        table { border-collapse: collapse; width: 100%; margin-bottom: 20px; }
        th, td { border: 1px solid #ddd; padding: 8px; text-align: left; }
        th { background-color: #f2f2f2; font-weight: bold; }
        .metric-value { font-weight: bold; color: #007acc; }
        .error-count { color: #d32f2f; font-weight: bold; }";
    }

    private void AddHtmlHeader(StringBuilder html, PerformanceReportModel report)
    {
        html.AppendLine("    <div class='header'>");
        html.AppendLine("        <h1>File Chunking System - Performance Report</h1>");
        html.AppendLine($"        <p><strong>Generated:</strong> {DateTime.Now:yyyy-MM-dd HH:mm:ss}</p>");
        html.AppendLine($"        <p><strong>Report Period:</strong> {report.ReportPeriod.StartDate:yyyy-MM-dd} to {report.ReportPeriod.EndDate:yyyy-MM-dd}</p>");
        html.AppendLine("    </div>");
    }

    private void AddHtmlGeneralStats(StringBuilder html, PerformanceReportModel report)
    {
        html.AppendLine("    <div class='section'>");
        html.AppendLine("        <h2>General Statistics</h2>");
        html.AppendLine("        <table>");
        html.AppendLine("            <tr><th>Metric</th><th>Value</th></tr>");
        html.AppendLine($"            <tr><td>Total Files Processed</td><td class='metric-value'>{report.TotalFilesProcessed:N0}</td></tr>");
        html.AppendLine($"            <tr><td>Total Bytes Processed</td><td class='metric-value'>{FormatBytes(report.TotalBytesProcessed)}</td></tr>");
        html.AppendLine($"            <tr><td>Total Chunks Created</td><td class='metric-value'>{report.TotalChunksCreated:N0}</td></tr>");
        html.AppendLine($"            <tr><td>Average Chunking Time</td><td class='metric-value'>{report.AverageChunkingTime:F2} ms</td></tr>");
        html.AppendLine($"            <tr><td>Average Merging Time</td><td class='metric-value'>{report.AverageMergingTime:F2} ms</td></tr>");
        html.AppendLine("        </table>");
        html.AppendLine("    </div>");
    }

    private void AddHtmlDetailedStats(StringBuilder html, PerformanceReportModel report)
    {
        // File Size Distribution
        if (report.FileSizeDistribution?.Any() == true)
        {
            html.AppendLine("    <div class='section'>");
            html.AppendLine("        <h2>File Size Distribution</h2>");
            html.AppendLine("        <table>");
            html.AppendLine("            <tr><th>Size Range</th><th>File Count</th><th>Percentage</th></tr>");
            
            var total = report.FileSizeDistribution.Values.Sum();
            foreach (var item in report.FileSizeDistribution)
            {
                var percentage = total > 0 ? (double)item.Value / total * 100 : 0;
                html.AppendLine($"            <tr><td>{item.Key}</td><td class='metric-value'>{item.Value:N0}</td><td>{percentage:F1}%</td></tr>");
            }
            
            html.AppendLine("        </table>");
            html.AppendLine("    </div>");
        }

        // Chunking Strategy Statistics
        if (report.ChunkingStrategyStats?.Any() == true)
        {
            html.AppendLine("    <div class='section'>");
            html.AppendLine("        <h2>Chunking Strategy Statistics</h2>");
            html.AppendLine("        <table>");
            html.AppendLine("            <tr><th>Strategy</th><th>File Count</th><th>Total Bytes</th><th>Avg Chunks</th><th>Avg Chunk Size</th></tr>");
            
            foreach (var strategy in report.ChunkingStrategyStats)
            {
                html.AppendLine($"            <tr>");
                html.AppendLine($"                <td>{strategy.Key}</td>");
                html.AppendLine($"                <td class='metric-value'>{strategy.Value.FileCount:N0}</td>");
                html.AppendLine($"                <td class='metric-value'>{FormatBytes(strategy.Value.TotalBytes)}</td>");
                html.AppendLine($"                <td class='metric-value'>{strategy.Value.AverageChunks:F1}</td>");
                html.AppendLine($"                <td class='metric-value'>{FormatBytes((long)strategy.Value.AverageChunkSize)}</td>");
                html.AppendLine($"            </tr>");
            }
            
            html.AppendLine("        </table>");
            html.AppendLine("    </div>");
        }

        // Storage Type Statistics
        if (report.StorageTypeStats?.Any() == true)
        {
            html.AppendLine("    <div class='section'>");
            html.AppendLine("        <h2>Storage Type Statistics</h2>");
            html.AppendLine("        <table>");
            html.AppendLine("            <tr><th>Storage Type</th><th>Chunk Count</th><th>Total Bytes</th><th>Avg Response Time</th><th>Error Count</th></tr>");
            
            foreach (var storage in report.StorageTypeStats)
            {
                var errorClass = storage.Value.ErrorCount > 0 ? "error-count" : "metric-value";
                html.AppendLine($"            <tr>");
                html.AppendLine($"                <td>{storage.Key}</td>");
                html.AppendLine($"                <td class='metric-value'>{storage.Value.ChunkCount:N0}</td>");
                html.AppendLine($"                <td class='metric-value'>{FormatBytes(storage.Value.TotalBytes)}</td>");
                html.AppendLine($"                <td class='metric-value'>{storage.Value.AverageResponseTime:F2} ms</td>");
                html.AppendLine($"                <td class='{errorClass}'>{storage.Value.ErrorCount:N0}</td>");
                html.AppendLine($"            </tr>");
            }
            
            html.AppendLine("        </table>");
            html.AppendLine("    </div>");
        }

        // Top Largest Files
        if (report.TopLargestFiles?.Any() == true)
        {
            html.AppendLine("    <div class='section'>");
            html.AppendLine("        <h2>Top Largest Files</h2>");
            html.AppendLine("        <table>");
            html.AppendLine("            <tr><th>Rank</th><th>File Name</th><th>File Size</th><th>Chunk Count</th><th>Strategy</th><th>Processed At</th></tr>");
            
            var counter = 1;
            foreach (var file in report.TopLargestFiles.Take(10))
            {
                var fileName = file.FileName.Length > 50 
                    ? file.FileName[..47] + "..." 
                    : file.FileName;

                html.AppendLine($"            <tr>");
                html.AppendLine($"                <td>{counter}</td>");
                html.AppendLine($"                <td title='{file.FileName}'>{fileName}</td>");
                html.AppendLine($"                <td class='metric-value'>{FormatBytes(file.FileSize)}</td>");
                html.AppendLine($"                <td class='metric-value'>{file.ChunkCount:N0}</td>");
                html.AppendLine($"                <td>{file.ChunkingStrategy ?? "Unknown"}</td>");
                html.AppendLine($"                <td>{file.ProcessedAt:yyyy-MM-dd HH:mm}</td>");
                html.AppendLine($"            </tr>");
                counter++;
            }
            
            html.AppendLine("        </table>");
            html.AppendLine("    </div>");
        }

        // Error Statistics
        if (report.ErrorStats?.TotalErrors > 0)
        {
            html.AppendLine("    <div class='section'>");
            html.AppendLine("        <h2>Error Statistics</h2>");
            html.AppendLine($"        <p><strong>Total Errors:</strong> <span class='error-count'>{report.ErrorStats.TotalErrors:N0}</span></p>");
            
            if (report.ErrorStats.ErrorsByType?.Any() == true)
            {
                html.AppendLine("        <table>");
                html.AppendLine("            <tr><th>Error Type</th><th>Count</th><th>Percentage</th></tr>");
                
                foreach (var error in report.ErrorStats.ErrorsByType.OrderByDescending(e => e.Value))
                {
                    var percentage = (double)error.Value / report.ErrorStats.TotalErrors * 100;
                    html.AppendLine($"            <tr>");
                    html.AppendLine($"                <td>{error.Key}</td>");
                    html.AppendLine($"                <td class='error-count'>{error.Value:N0}</td>");
                    html.AppendLine($"                <td>{percentage:F1}%</td>");
                    html.AppendLine($"            </tr>");
                }
                
                html.AppendLine("        </table>");
            }
            
            html.AppendLine("    </div>");
        }
    }

    private void AddTextGeneralStats(StringBuilder text, PerformanceReportModel report)
    {
        text.AppendLine("GENERAL STATISTICS");
        text.AppendLine("------------------");
        text.AppendLine($"Total Files Processed: {report.TotalFilesProcessed:N0}");
        text.AppendLine($"Total Bytes Processed: {FormatBytes(report.TotalBytesProcessed)}");
        text.AppendLine($"Total Chunks Created: {report.TotalChunksCreated:N0}");
        text.AppendLine($"Average Chunking Time: {report.AverageChunkingTime:F2} ms");
        text.AppendLine($"Average Merging Time: {report.AverageMergingTime:F2} ms");
        text.AppendLine();
    }

    private void AddTextDetailedStats(StringBuilder text, PerformanceReportModel report)
    {
        // File Size Distribution
        if (report.FileSizeDistribution?.Any() == true)
        {
            text.AppendLine("FILE SIZE DISTRIBUTION");
            text.AppendLine("----------------------");
            
            var total = report.FileSizeDistribution.Values.Sum();
            foreach (var item in report.FileSizeDistribution)
            {
                var percentage = total > 0 ? (double)item.Value / total * 100 : 0;
                text.AppendLine($"{item.Key,-20}: {item.Value,8:N0} files ({percentage,5:F1}%)");
            }
            text.AppendLine();
        }

        // Chunking Strategy Statistics
        if (report.ChunkingStrategyStats?.Any() == true)
        {
            text.AppendLine("CHUNKING STRATEGY STATISTICS");
            text.AppendLine("----------------------------");
            text.AppendLine($"{"Strategy",-20} {"Files",8} {"Total Bytes",15} {"Avg Chunks",12} {"Avg Chunk Size",15}");
            text.AppendLine(new string('-', 70));
            
            foreach (var strategy in report.ChunkingStrategyStats)
            {
                text.AppendLine($"{strategy.Key,-20} {strategy.Value.FileCount,8:N0} {FormatBytes(strategy.Value.TotalBytes),15} " +
                            $"{strategy.Value.AverageChunks,12:F1} {FormatBytes((long)strategy.Value.AverageChunkSize),15}");
            }
            text.AppendLine();
        }

        // Storage Type Statistics
        if (report.StorageTypeStats?.Any() == true)
        {
            text.AppendLine("STORAGE TYPE STATISTICS");
            text.AppendLine("-----------------------");
            text.AppendLine($"{"Storage Type",-15} {"Chunks",8} {"Total Bytes",15} {"Avg Response",12} {"Errors",8}");
            text.AppendLine(new string('-', 58));
            
            foreach (var storage in report.StorageTypeStats)
            {
                var errorIndicator = storage.Value.ErrorCount > 0 ? " (!)" : "";
                text.AppendLine($"{storage.Key,-15} {storage.Value.ChunkCount,8:N0} {FormatBytes(storage.Value.TotalBytes),15} " +
                            $"{storage.Value.AverageResponseTime,9:F2} ms {storage.Value.ErrorCount,8:N0}{errorIndicator}");
            }
            text.AppendLine();
        }

        // Top Largest Files
        if (report.TopLargestFiles?.Any() == true)
        {
            text.AppendLine("TOP LARGEST FILES");
            text.AppendLine("-----------------");
            text.AppendLine($"{"Rank",4} {"File Name",-40} {"Size",12} {"Chunks",8} {"Strategy",-15} {"Processed At",-16}");
            text.AppendLine(new string('-', 95));
            
            var counter = 1;
            foreach (var file in report.TopLargestFiles.Take(10))
            {
                var fileName = file.FileName.Length > 40 
                    ? file.FileName[..37] + "..." 
                    : file.FileName;

                text.AppendLine($"{counter,4} {fileName,-40} {FormatBytes(file.FileSize),12} {file.ChunkCount,8:N0} " +
                            $"{(file.ChunkingStrategy ?? "Unknown"),-15} {file.ProcessedAt,-16:yyyy-MM-dd HH:mm}");
                counter++;
            }
            text.AppendLine();
        }

        // Error Statistics
        if (report.ErrorStats?.TotalErrors > 0)
        {
            text.AppendLine("ERROR STATISTICS");
            text.AppendLine("----------------");
            text.AppendLine($"Total Errors: {report.ErrorStats.TotalErrors:N0}");
            text.AppendLine();
            
            if (report.ErrorStats.ErrorsByType?.Any() == true)
            {
                text.AppendLine("Error Breakdown:");
                text.AppendLine($"{"Error Type",-30} {"Count",8} {"Percentage",10}");
                text.AppendLine(new string('-', 48));
                
                foreach (var error in report.ErrorStats.ErrorsByType.OrderByDescending(e => e.Value))
                {
                    var percentage = (double)error.Value / report.ErrorStats.TotalErrors * 100;
                    text.AppendLine($"{error.Key,-30} {error.Value,8:N0} {percentage,9:F1}%");
                }
            }
            text.AppendLine();
        }

        // Summary and Recommendations
        text.AppendLine("SUMMARY AND RECOMMENDATIONS");
        text.AppendLine("---------------------------");
        
        // Generate automatic recommendations based on the data
        AddRecommendations(text, report);
    }

    private void AddRecommendations(StringBuilder text, PerformanceReportModel report)
    {
        var recommendations = new List<string>();

        // Performance-based recommendations
        if (report.AverageChunkingTime > 1000) // More than 1 second
            recommendations.Add("• Consider optimizing chunking algorithm - average chunking time is high");

        if (report.AverageMergingTime > 2000) // More than 2 seconds
            recommendations.Add("• Consider optimizing merging process - average merging time is high");

        // Error-based recommendations
        if (report.ErrorStats?.TotalErrors > 0)
        {
            var errorRate = (double)report.ErrorStats.TotalErrors / report.TotalFilesProcessed * 100;
            if (errorRate > 5)
            {
                recommendations.Add($"• High error rate detected ({errorRate:F1}%) - investigate common error patterns");
            }
        }

        // Storage-based recommendations
        if (report.StorageTypeStats?.Any() == true)
        {
            var slowStorageTypes = report.StorageTypeStats
                .Where(s => s.Value.AverageResponseTime > 500)
                .Select(s => s.Key);
            
            if (slowStorageTypes.Any())
            {
                recommendations.Add($"• Slow storage performance detected in: {string.Join(", ", slowStorageTypes)}");
            }
        }

        // File size-based recommendations
        if (report.FileSizeDistribution?.ContainsKey("Very Large (>1GB)") == true && 
            report.FileSizeDistribution["Very Large (>1GB)"] > 0)
        {
            recommendations.Add("• Consider implementing parallel processing for very large files");
        }

        // General recommendations
        if (report.TotalFilesProcessed > 1000 && report.AverageChunkingTime > 500)
            recommendations.Add("• Consider implementing caching mechanisms for frequently accessed files");

        if (recommendations.Any())
        {
            text.AppendLine("Recommendations:");
            foreach (var recommendation in recommendations)
            {
                text.AppendLine(recommendation);
            }
        }
        else
        {
            text.AppendLine("• System performance appears to be within acceptable parameters");
            text.AppendLine("• Continue monitoring for performance trends");
        }

        text.AppendLine();
        text.AppendLine($"Report generated on: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        text.AppendLine("End of Report");
    }
}

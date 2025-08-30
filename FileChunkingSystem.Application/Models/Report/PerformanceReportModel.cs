namespace FileChunkingSystem.Application.Models;

public class PerformanceReportModel : BaseModel
{
    public DateRange ReportPeriod { get; set; } = new();
    public int TotalFilesProcessed { get; set; }
    public long TotalBytesProcessed { get; set; }
    public int TotalChunksCreated { get; set; }
    public double AverageChunkingTime { get; set; }
    public double AverageMergingTime { get; set; }
    public Dictionary<string, ChunkingStrategyStats> ChunkingStrategyStats { get; set; } = new();
    public Dictionary<string, StorageTypeStats> StorageTypeStats { get; set; } = new();
    public Dictionary<string, int> FileSizeDistribution { get; set; } = new();
    public Dictionary<DateTime, DailyPerformanceStats> PerformanceByDay { get; set; } = new();
    public List<FileStats> TopLargestFiles { get; set; } = new();
    public ErrorStats ErrorStats { get; set; } = new();
}

public class DateRange : BaseModel
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

public class ChunkingStrategyStats : BaseModel
{
    public int FileCount { get; set; }
    public long TotalBytes { get; set; }
    public double AverageChunks { get; set; }
    public double AverageChunkSize { get; set; }
}

public class StorageTypeStats : BaseModel
{
    public int ChunkCount { get; set; }
    public long TotalBytes { get; set; }
    public double AverageResponseTime { get; set; }
    public int ErrorCount { get; set; }
}

public class DailyPerformanceStats : BaseModel
{
    public int FilesProcessed { get; set; }
    public long TotalBytes { get; set; }
    public int TotalChunks { get; set; }
}

public class FileStats : BaseModel
{
    public string FileName { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public int ChunkCount { get; set; }
    public string ChunkingStrategy { get; set; } = string.Empty;
    public DateTime ProcessedAt { get; set; }
}

public class ErrorStats : BaseModel
{
    public int TotalErrors { get; set; }
    public Dictionary<string, int> ErrorsByType { get; set; } = new();
}

namespace FileChunkingSystem.Application.Models;

/// <summary>
/// Model representing a comprehensive performance report for the file chunking system
/// </summary>
public class PerformanceReportModel : BaseModel
{
    /// <summary>
    /// Gets or sets the date range covered by this performance report
    /// </summary>
    public DateRange ReportPeriod { get; set; } = new();
    
    /// <summary>
    /// Gets or sets the total number of files processed during the report period
    /// </summary>
    public int TotalFilesProcessed { get; set; }
    
    /// <summary>
    /// Gets or sets the total number of bytes processed during the report period
    /// </summary>
    public long TotalBytesProcessed { get; set; }
    
    /// <summary>
    /// Gets or sets the total number of chunks created during the report period
    /// </summary>
    public int TotalChunksCreated { get; set; }
    
    /// <summary>
    /// Gets or sets the average time taken for chunking operations in milliseconds
    /// </summary>
    public double AverageChunkingTime { get; set; }
    
    /// <summary>
    /// Gets or sets the average time taken for merging operations in milliseconds
    /// </summary>
    public double AverageMergingTime { get; set; }
    
    /// <summary>
    /// Gets or sets the performance statistics grouped by chunking strategy
    /// </summary>
    public Dictionary<string, ChunkingStrategyStats> ChunkingStrategyStats { get; set; } = new();
    
    /// <summary>
    /// Gets or sets the performance statistics grouped by storage type
    /// </summary>
    public Dictionary<string, StorageTypeStats> StorageTypeStats { get; set; } = new();
    
    /// <summary>
    /// Gets or sets the distribution of files by size ranges
    /// </summary>
    public Dictionary<string, int> FileSizeDistribution { get; set; } = new();
    
    /// <summary>
    /// Gets or sets the daily performance statistics indexed by date
    /// </summary>
    public Dictionary<DateTime, DailyPerformanceStats> PerformanceByDay { get; set; } = new();
    
    /// <summary>
    /// Gets or sets the list of largest files processed during the report period
    /// </summary>
    public List<FileStats> TopLargestFiles { get; set; } = new();
    
    /// <summary>
    /// Gets or sets the error statistics for the report period
    /// </summary>
    public ErrorStats ErrorStats { get; set; } = new();
}

/// <summary>
/// Model representing a date range with start and end dates
/// </summary>
public class DateRange : BaseModel
{
    /// <summary>
    /// Gets or sets the start date of the range
    /// </summary>
    public DateTime StartDate { get; set; }
    
    /// <summary>
    /// Gets or sets the end date of the range
    /// </summary>
    public DateTime EndDate { get; set; }
}

/// <summary>
/// Model representing performance statistics for a specific chunking strategy
/// </summary>
public class ChunkingStrategyStats : BaseModel
{
    /// <summary>
    /// Gets or sets the number of files processed using this chunking strategy
    /// </summary>
    public int FileCount { get; set; }
    
    /// <summary>
    /// Gets or sets the total number of bytes processed using this chunking strategy
    /// </summary>
    public long TotalBytes { get; set; }
    
    /// <summary>
    /// Gets or sets the average number of chunks created per file using this strategy
    /// </summary>
    public double AverageChunks { get; set; }
    
    /// <summary>
    /// Gets or sets the average size of chunks created using this strategy in bytes
    /// </summary>
    public double AverageChunkSize { get; set; }
}

/// <summary>
/// Model representing performance statistics for a specific storage type
/// </summary>
public class StorageTypeStats : BaseModel
{
    /// <summary>
    /// Gets or sets the number of chunks stored using this storage type
    /// </summary>
    public int ChunkCount { get; set; }
    
    /// <summary>
    /// Gets or sets the total number of bytes stored using this storage type
    /// </summary>
    public long TotalBytes { get; set; }
    
    /// <summary>
    /// Gets or sets the average response time for operations using this storage type in milliseconds
    /// </summary>
    public double AverageResponseTime { get; set; }
    
    /// <summary>
    /// Gets or sets the number of errors encountered with this storage type
    /// </summary>
    public int ErrorCount { get; set; }
}

/// <summary>
/// Model representing performance statistics for a single day
/// </summary>
public class DailyPerformanceStats : BaseModel
{
    /// <summary>
    /// Gets or sets the number of files processed on this day
    /// </summary>
    public int FilesProcessed { get; set; }
    
    /// <summary>
    /// Gets or sets the total number of bytes processed on this day
    /// </summary>
    public long TotalBytes { get; set; }
    
    /// <summary>
    /// Gets or sets the total number of chunks created on this day
    /// </summary>
    public int TotalChunks { get; set; }
}

/// <summary>
/// Model representing statistics for an individual file
/// </summary>
public class FileStats : BaseModel
{
    /// <summary>
    /// Gets or sets the name of the file
    /// </summary>
    public string FileName { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the size of the file in bytes
    /// </summary>
    public long FileSize { get; set; }
    
    /// <summary>
    /// Gets or sets the number of chunks the file was divided into
    /// </summary>
    public int ChunkCount { get; set; }
    
    /// <summary>
    /// Gets or sets the chunking strategy used for this file
    /// </summary>
    public string ChunkingStrategy { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the timestamp when the file was processed
    /// </summary>
    public DateTime ProcessedAt { get; set; }
}

/// <summary>
/// Model representing error statistics for the reporting period
/// </summary>
public class ErrorStats : BaseModel
{
    /// <summary>
    /// Gets or sets the total number of errors that occurred
    /// </summary>
    public int TotalErrors { get; set; }
    
    /// <summary>
    /// Gets or sets the breakdown of errors by error type
    /// </summary>
    public Dictionary<string, int> ErrorsByType { get; set; } = new();
}

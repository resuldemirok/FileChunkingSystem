using FileChunkingSystem.Application.Interfaces;
using FileChunkingSystem.Application.Models;
using FileChunkingSystem.Domain.Entities;
using FileChunkingSystem.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace FileChunkingSystem.Application.Services;

public class PerformanceTrackingService : IPerformanceTrackingService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<PerformanceTrackingService> _logger;

    public PerformanceTrackingService(IUnitOfWork unitOfWork, ILogger<PerformanceTrackingService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<PerformanceReportModel> GeneratePerformanceReportAsync(
        DateTime? startDate = null, 
        DateTime? endDate = null)
    {
        try
        {
            var start = startDate ?? DateTime.UtcNow.AddDays(-30);
            var end = endDate ?? DateTime.UtcNow;

            var allFileMetadatas = await _unitOfWork.Repository<FileMetadata>().GetAllAsync();
            var fileMetadatas = allFileMetadatas.Where(f => f.CreatedAt >= start && f.CreatedAt <= end).ToList();

            var allPerformanceLogs = await _unitOfWork.Repository<PerformanceLog>().GetAllAsync();
            var performanceLogs = allPerformanceLogs.Where(p => p.Timestamp >= start && p.Timestamp <= end).ToList();

            return new PerformanceReportModel
            {
                ReportPeriod = new DateRange { StartDate = start, EndDate = end },
                TotalFilesProcessed = fileMetadatas.Count,
                TotalBytesProcessed = fileMetadatas.Sum(f => f.FileSize),
                TotalChunksCreated = fileMetadatas.Sum(f => f.TotalChunks),
                AverageChunkingTime = performanceLogs
                    .Where(p => p.OperationType == "Chunking")
                    .DefaultIfEmpty()
                    .Average(p => p?.DurationMs ?? 0),
                AverageMergingTime = performanceLogs
                    .Where(p => p.OperationType == "Merging")
                    .DefaultIfEmpty()
                    .Average(p => p?.DurationMs ?? 0),
                ChunkingStrategyStats = GetChunkingStrategyStats(fileMetadatas),
                StorageTypeStats = await GetStorageTypeStatsAsync(fileMetadatas),
                FileSizeDistribution = GetFileSizeDistribution(fileMetadatas),
                PerformanceByDay = GetDailyPerformance(fileMetadatas, start, end),
                TopLargestFiles = GetTopLargestFiles(fileMetadatas, 10),
                ErrorStats = GetErrorStats(performanceLogs)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating performance report");
            throw;
        }
    }

    private Dictionary<string, ChunkingStrategyStats> GetChunkingStrategyStats(List<FileMetadata> files)
    {
        return files.GroupBy(f => f.ChunkingAlgorithm.ToString())
            .ToDictionary(g => g.Key, g => new ChunkingStrategyStats
            {
                FileCount = g.Count(),
                TotalBytes = g.Sum(f => f.FileSize),
                AverageChunks = g.Average(f => f.TotalChunks),
                AverageChunkSize = g.Average(f => f.ChunkSize)
            });
    }

    private async Task<Dictionary<string, StorageTypeStats>> GetStorageTypeStatsAsync(List<FileMetadata> files)
    {
        var result = new Dictionary<string, StorageTypeStats>();
        
        var allChunks = new List<ChunkMetadata>();
        foreach (var file in files)
        {
            var chunks = await _unitOfWork.Repository<ChunkMetadata>().GetAllAsync();
            var fileChunks = chunks.Where(c => c.FileMetadataId == file.Id);
            allChunks.AddRange(fileChunks);
        }
        
        var groupedChunks = allChunks.GroupBy(c => c.StorageProviderType.ToString());
        
        foreach (var group in groupedChunks)
        {
            result[group.Key] = new StorageTypeStats
            {
                ChunkCount = group.Count(),
                TotalBytes = group.Sum(c => c.ChunkSize),
                AverageResponseTime = 0,
                ErrorCount = 0
            };
        }
        
        return result;
    }

    private Dictionary<string, int> GetFileSizeDistribution(List<FileMetadata> files)
    {
        return new Dictionary<string, int>
        {
            ["< 1 MB"] = files.Count(f => f.FileSize < 1024 * 1024),
            ["1-10 MB"] = files.Count(f => f.FileSize >= 1024 * 1024 && f.FileSize < 10 * 1024 * 1024),
            ["10-100 MB"] = files.Count(f => f.FileSize >= 10 * 1024 * 1024 && f.FileSize < 100 * 1024 * 1024),
            ["> 100 MB"] = files.Count(f => f.FileSize >= 100 * 1024 * 1024)
        };
    }

    private Dictionary<DateTime, DailyPerformanceStats> GetDailyPerformance(
        List<FileMetadata> files, DateTime start, DateTime end)
    {
        return files.GroupBy(f => f.CreatedAt.Date)
            .ToDictionary(g => g.Key, g => new DailyPerformanceStats
            {
                FilesProcessed = g.Count(),
                TotalBytes = g.Sum(f => f.FileSize),
                TotalChunks = g.Sum(f => f.TotalChunks)
            });
    }

    private List<FileStats> GetTopLargestFiles(List<FileMetadata> files, int count)
    {
        return files.OrderByDescending(f => f.FileSize)
            .Take(count)
            .Select(f => new FileStats
            {
                FileName = f.OriginalFileName,
                FileSize = f.FileSize,
                ChunkCount = f.TotalChunks,
                ChunkingStrategy = f.ChunkingAlgorithm.ToString(),
                ProcessedAt = f.CreatedAt
            }).ToList();
    }

    private ErrorStats GetErrorStats(List<PerformanceLog> logs)
    {
        var errorLogs = logs.Where(l => !string.IsNullOrEmpty(l.ErrorMessage));
        return new ErrorStats
        {
            TotalErrors = errorLogs.Count(),
            ErrorsByType = errorLogs.GroupBy(l => l.ErrorMessage ?? "Unknown")
                .ToDictionary(g => g.Key, g => g.Count())
        };
    }
}

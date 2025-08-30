using FileChunkingSystem.Application.Models;

namespace FileChunkingSystem.Application.Interfaces;

public interface IPerformanceTrackingService
{
    Task<PerformanceReportModel> GeneratePerformanceReportAsync(DateTime? startDate = null, DateTime? endDate = null);
}

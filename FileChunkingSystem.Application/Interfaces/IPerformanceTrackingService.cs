using FileChunkingSystem.Application.Models;

namespace FileChunkingSystem.Application.Interfaces;

/// <summary>
/// Service interface for tracking and reporting system performance metrics
/// </summary>
public interface IPerformanceTrackingService
{
    /// <summary>
    /// Generates a comprehensive performance report for the specified date range
    /// </summary>
    /// <param name="startDate">The start date for the report (optional, defaults to system start)</param>
    /// <param name="endDate">The end date for the report (optional, defaults to current date)</param>
    /// <returns>A detailed performance report model</returns>
    Task<PerformanceReportModel> GeneratePerformanceReportAsync(DateTime? startDate = null, DateTime? endDate = null);
}

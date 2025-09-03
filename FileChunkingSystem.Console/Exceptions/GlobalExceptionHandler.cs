using FileChunkingSystem.Domain.Entities;
using FileChunkingSystem.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using FileChunkingSystem.Domain.Interfaces;

namespace FileChunkingSystem.Console.Exceptions;

/// <summary>
/// Interface for handling global exceptions throughout the application.
/// </summary>
public interface IGlobalExceptionHandler
{
    /// <summary>
    /// Handles exceptions asynchronously with optional context information.
    /// </summary>
    /// <param name="exception">The exception to handle</param>
    /// <param name="context">Optional context information about where the exception occurred</param>
    /// <returns>A task representing the asynchronous operation</returns>
    Task HandleExceptionAsync(Exception exception, string? context = null);

    /// <summary>
    /// Handles exceptions synchronously with optional context information.
    /// </summary>
    /// <param name="exception">The exception to handle</param>
    /// <param name="context">Optional context information about where the exception occurred</param>
    void HandleException(Exception exception, string? context = null);
}

/// <summary>
/// Global exception handler implementation that provides centralized exception handling with logging and user notification capabilities.
/// </summary>
public class GlobalExceptionHandler : IGlobalExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IServiceScopeFactory serviceScopeFactory)
    {
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
    }

    public async Task HandleExceptionAsync(Exception exception, string? context = null)
    {
        var errorId = Guid.NewGuid();
        _logger.LogError(exception, "Unhandled exception occurred. ErrorId: {ErrorId}, Context: {Context}", 
            errorId, context ?? "Unknown");

        // Log to database in background
        await LogToDatabaseAsync(exception, context, errorId);
    }

    public void HandleException(Exception exception, string? context = null)
    {
        _ = HandleExceptionAsync(exception, context);
    }

    private async Task LogToDatabaseAsync(Exception exception, string? context, Guid errorId)
    {
        try
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            var errorLog = new ErrorLog
            {
                Id = errorId,
                Message = exception.Message,
                StackTrace = exception.StackTrace ?? string.Empty,
                Source = context ?? exception.Source ?? "Console Application",
                Timestamp = DateTime.UtcNow
            };

            await unitOfWork.Repository<ErrorLog>().AddAsync(errorLog);
            await unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Exception logged to database with ID: {ErrorId}", errorId);
        }
        catch (Exception logEx)
        {
            _logger.LogError(logEx, "Failed to log exception to database. Original ErrorId: {ErrorId}", errorId);
        }
    }
}

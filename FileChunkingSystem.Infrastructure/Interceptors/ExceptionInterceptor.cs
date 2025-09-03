using Castle.DynamicProxy;
using FileChunkingSystem.Domain.Entities;
using FileChunkingSystem.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace FileChunkingSystem.Infrastructure.Interceptors;

/// <summary>
/// Castle DynamicProxy interceptor for handling and logging exceptions across the application.
/// Automatically logs exceptions to both application logger and database.
/// </summary>
public class ExceptionInterceptor : IInterceptor
{
    private readonly ILogger<ExceptionInterceptor> _logger;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes a new instance of the ExceptionInterceptor
    /// </summary>
    /// <param name="logger">Logger for application logging</param>
    /// <param name="unitOfWork">Unit of work for database operations</param>
    public ExceptionInterceptor(ILogger<ExceptionInterceptor> logger, IUnitOfWork unitOfWork)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Intercepts method calls and handles exceptions by logging them
    /// </summary>
    /// <param name="invocation">The method invocation context</param>
    public void Intercept(IInvocation invocation)
    {
        try
        {
            // Proceed with the original method call
            invocation.Proceed();
        }
        catch (Exception ex)
        {
            // Log exception to application logger
            _logger.LogError(ex, "Exception occurred in {Method}", invocation.Method.Name);
            
            // Log exception to database asynchronously (fire and forget)
            _ = Task.Run(async () =>
            {
                try
                {
                    var errorLog = new ErrorLog
                    {
                        Message = ex.Message,
                        StackTrace = ex.StackTrace ?? string.Empty,
                        Source = $"{invocation.TargetType?.Name}.{invocation.Method.Name}",
                        Timestamp = DateTime.UtcNow
                    };

                    await _unitOfWork.Repository<ErrorLog>().AddAsync(errorLog);
                    await _unitOfWork.SaveChangesAsync();
                }
                catch (Exception logEx)
                {
                    // If database logging fails, log to application logger
                    _logger.LogError(logEx, "Failed to log exception to database");
                }
            });

            // Re-throw the original exception
            throw;
        }
    }
}

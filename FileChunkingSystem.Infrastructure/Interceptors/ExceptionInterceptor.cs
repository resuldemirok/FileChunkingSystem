using Castle.DynamicProxy;
using FileChunkingSystem.Domain.Entities;
using FileChunkingSystem.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace FileChunkingSystem.Infrastructure.Interceptors;

public class ExceptionInterceptor : IInterceptor
{
    private readonly ILogger<ExceptionInterceptor> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public ExceptionInterceptor(ILogger<ExceptionInterceptor> logger, IUnitOfWork unitOfWork)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public void Intercept(IInvocation invocation)
    {
        try
        {
            invocation.Proceed();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred in {Method}", invocation.Method.Name);
            
            // Log to database
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
                    _logger.LogError(logEx, "Failed to log exception to database");
                }
            });

            throw;
        }
    }
}

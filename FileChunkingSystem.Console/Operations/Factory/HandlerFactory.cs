using FileChunkingSystem.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using FileChunkingSystem.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace FileChunkingSystem.Console.Handlers.Factory;

public class HandlerFactory
{
    private readonly IServiceProvider _serviceProvider;

    public HandlerFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public FileUploadHandler CreateFileUploadHandler()
    {
        return new FileUploadHandler(
            _serviceProvider.GetRequiredService<IFileChunkingService>(),
            _serviceProvider.GetRequiredService<IPerformanceTrackingService>(),
            _serviceProvider.GetRequiredService<ILogger<FileUploadHandler>>());
    }

    public FileMergeHandler CreateFileMergeHandler()
    {
        return new FileMergeHandler(
            _serviceProvider.GetRequiredService<IFileChunkingService>(),
            _serviceProvider.GetRequiredService<IPerformanceTrackingService>(),
            _serviceProvider.GetRequiredService<ILogger<FileMergeHandler>>());
    }

    public FileDeleteHandler CreateFileDeleteHandler()
    {
        return new FileDeleteHandler(
            _serviceProvider.GetRequiredService<IFileChunkingService>(),
            _serviceProvider.GetRequiredService<IPerformanceTrackingService>(),
            _serviceProvider.GetRequiredService<ILogger<FileDeleteHandler>>());
    }

    public FileListHandler CreateFileListHandler()
    {
        return new FileListHandler(
            _serviceProvider.GetRequiredService<IUnitOfWork>(),
            _serviceProvider.GetRequiredService<IFileChunkingService>(),
            _serviceProvider.GetRequiredService<IPerformanceTrackingService>(),
            _serviceProvider.GetRequiredService<ILogger<FileListHandler>>());
    }

    public PerformanceReportHandler CreatePerformanceReportHandler()
    {
        return new PerformanceReportHandler(
            _serviceProvider.GetRequiredService<IFileChunkingService>(),
            _serviceProvider.GetRequiredService<IPerformanceTrackingService>(),
            _serviceProvider.GetRequiredService<ILogger<PerformanceReportHandler>>());
    }
}

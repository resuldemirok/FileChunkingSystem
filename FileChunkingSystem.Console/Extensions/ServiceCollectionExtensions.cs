using Castle.DynamicProxy;
using FileChunkingSystem.Application.Interfaces;
using FileChunkingSystem.Application.Services;
using FileChunkingSystem.Domain.Interfaces;
using FileChunkingSystem.Infrastructure.Data;
using FileChunkingSystem.Infrastructure.Interceptors;
using FileChunkingSystem.Infrastructure.Repositories;
using FileChunkingSystem.Infrastructure.Services;
using FileChunkingSystem.Infrastructure.Storage;
using FileChunkingSystem.Infrastructure.Strategies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using FileChunkingSystem.Console.Handlers;
using FileChunkingSystem.Console.Handlers.Factory;

namespace FileChunkingSystem.Console.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Database Context
        services.AddDbContext<MetadataDbContext>(options =>
            options.UseLazyLoadingProxies().UseNpgsql(configuration.GetConnectionString("MetadataConnection")));
        services.AddDbContext<StorageDbContext>(options =>
            options.UseLazyLoadingProxies().UseNpgsql(configuration.GetConnectionString("PostgreSQL")));

        // Repositories
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

        // Storage Providers
        services.AddScoped<IStorageProvider, FileSystemStorageProvider>();
        services.AddScoped<IStorageProvider, PostgreSQLStorageProvider>();
        services.AddScoped<IStorageProvider, MongoDBStorageProvider>();

        // Chunking Strategies
        services.AddScoped<IChunkingStrategy, RoundRobinChunkingStrategy>();
        services.AddScoped<IChunkingStrategy, CustomInterleavedChunkingStrategy>();
        services.AddScoped<IChunkingStrategy, SequentialBlocksChunkingStrategy>();

        // AOP Interceptor
        services.AddSingleton<ProxyGenerator>();
        services.AddScoped<ExceptionInterceptor>();

        // Performance Tracking
        services.AddScoped<IPerformanceTrackingService, PerformanceTrackingService>();

        // Handlers
        services.AddScoped<HandlerFactory>();
        services.AddScoped<FileUploadHandler>();
        services.AddScoped<FileMergeHandler>();
        services.AddScoped<FileDeleteHandler>();
        services.AddScoped<FileListHandler>();
        services.AddScoped<PerformanceReportHandler>();

        // FileChunkingService with AOP Proxy
        services.AddScoped<IFileChunkingService>(provider =>
        {
            var unitOfWork = provider.GetRequiredService<IUnitOfWork>();
            var storageProviders = provider.GetRequiredService<IEnumerable<IStorageProvider>>();
            var chunkingStrategies = provider.GetRequiredService<IEnumerable<IChunkingStrategy>>();
            var logger = provider.GetRequiredService<ILogger<FileChunkingService>>();

            // Create concrete service directly
            var concreteService = new FileChunkingService(
                unitOfWork,
                storageProviders,
                chunkingStrategies,
                logger);

            // Create proxy with interceptor
            var proxyGenerator = provider.GetRequiredService<ProxyGenerator>();
            var interceptor = provider.GetRequiredService<ExceptionInterceptor>();
            
            return proxyGenerator.CreateInterfaceProxyWithTarget<IFileChunkingService>(
                concreteService, 
                interceptor);
        });

        return services;
    }

    public static IServiceCollection AddLogging(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.AddConsole();
            builder.AddConfiguration(configuration.GetSection("Logging"));
        });

        return services;
    }
}

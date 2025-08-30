using FileChunkingSystem.Application.Interfaces;
using FileChunkingSystem.Console.Extensions;
using FileChunkingSystem.Console.Services;
using FileChunkingSystem.Infrastructure.Data;
using FileChunkingSystem.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FileChunkingSystem.Application.Mappings;
using FileChunkingSystem.Console.Exceptions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Mapster;

namespace FileChunkingSystem.Console;

class Program
{
    private static IGlobalExceptionHandler? _globalExceptionHandler;
    private static bool _isDevelopment;

    static async Task Main(string[] args)
    {
        var configuration = GetConfiguration();

        // Configure Serilog
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
            .MinimumLevel.Override("System", Serilog.Events.LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
            .WriteTo.File(
                path: "logs/app-.log",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30,
                outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();

        var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Development";
        _isDevelopment = environment.Equals("Development", StringComparison.OrdinalIgnoreCase);

        // Setup global exception handlers only in development
        if (_isDevelopment)
        {
            Log.Information("Development environment detected. Setting up global exception handler.");
            SetupGlobalExceptionHandler();
        }

        try
        {
            Log.Information("Starting File Chunking System");

            var host = CreateHostBuilder(args, configuration).Build();

            // Initialize global exception handler
            using var scope = host.Services.CreateScope();
            _globalExceptionHandler = scope.ServiceProvider.GetRequiredService<IGlobalExceptionHandler>();

            // Initialize all databases (PostgreSQL creation + EF Core migrations + MongoDB)
            await InitializeDatabasesAsync(host);

            // Register Mapster mappings
            MapsterConfig.RegisterMappings();

            // Run the application
            await RunApplicationAsync(host);
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application terminated unexpectedly");
        }
        finally
        {
            await Log.CloseAndFlushAsync();
        }
    }

    static void SetupGlobalExceptionHandler()
    {
        // Handle unhandled exceptions in the current AppDomain
        AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
        {
            var exception = e.ExceptionObject as Exception ?? new Exception("Unknown exception occurred");
            Log.Fatal(exception, "Unhandled AppDomain exception. IsTerminating: {IsTerminating}", e.IsTerminating);
            
            if (_globalExceptionHandler != null)
                _globalExceptionHandler.HandleException(exception, "AppDomain.UnhandledException");
        };

        // Handle unhandled exceptions in tasks
        TaskScheduler.UnobservedTaskException += (sender, e) =>
        {
            Log.Error(e.Exception, "Unobserved task exception");
            
            if (_globalExceptionHandler != null)
                _globalExceptionHandler.HandleException(e.Exception, "TaskScheduler.UnobservedTaskException");
            
            e.SetObserved(); // Prevent the process from terminating
        };
    }

    static IHostBuilder CreateHostBuilder(string[] args, IConfiguration configuration) =>
        Host.CreateDefaultBuilder(args)
            .UseSerilog()
            .ConfigureAppConfiguration((context, config) =>
            {
                config.AddConfiguration(configuration);
            })
            .ConfigureServices((context, services) =>
            {
                services.AddApplicationServices(context.Configuration);
                services.AddScoped<ConsoleApplication>();
                services.AddScoped<IDatabaseInitializer, DatabaseInitializer>();
                services.AddScoped<IGlobalExceptionHandler, GlobalExceptionHandler>();
            });

    static IConfiguration GetConfiguration()
    {
        var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production";
        
        return new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();
    }

    static async Task InitializeDatabasesAsync(IHost host)
    {
        using var scope = host.Services.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        
        try
        {
            logger.LogInformation("Starting database initialization process...");
            
            var databaseInitializer = scope.ServiceProvider.GetRequiredService<IDatabaseInitializer>();
            databaseInitializer.Initialize();
            
            logger.LogInformation("All databases initialized successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Critical error during database initialization");
            
            if (_isDevelopment && _globalExceptionHandler != null)
                await _globalExceptionHandler.HandleExceptionAsync(ex, "Database Initialization");
            
            throw;
        }
    }

    static async Task RunApplicationAsync(IHost host)
    {
        using var scope = host.Services.CreateScope();
        var app = scope.ServiceProvider.GetRequiredService<ConsoleApplication>();
        var exceptionHandler = scope.ServiceProvider.GetRequiredService<IGlobalExceptionHandler>();
        
        try
        {
            await app.RunAsync();
        }
        catch (Exception ex)
        {
            await exceptionHandler.HandleExceptionAsync(ex, "Console Application Execution");
            throw;
        }
    }
}

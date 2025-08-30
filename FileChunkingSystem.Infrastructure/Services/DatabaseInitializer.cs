using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using FileChunkingSystem.Infrastructure.Data;
using MongoDB.Driver;
using Npgsql;

namespace FileChunkingSystem.Infrastructure.Services;

public interface IDatabaseInitializer
{
    void Initialize();
}

public class DatabaseInitializer : IDatabaseInitializer
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<DatabaseInitializer> _logger;
    private readonly MetadataDbContext _metadataContext;
    private readonly StorageDbContext _storageContext;

    public DatabaseInitializer(
        IConfiguration configuration, 
        ILogger<DatabaseInitializer> logger,
        MetadataDbContext metadataContext,
        StorageDbContext storageContext)
    {
        _configuration = configuration;
        _logger = logger;
        _metadataContext = metadataContext;
        _storageContext = storageContext;
    }

    public void Initialize()
    {
        InitializeMetadataDatabase();
        InitializeStorageDatabase();
        InitializeMongoDBDatabases();
    }

    private void InitializeMetadataDatabase()
    {
        try
        {
            _logger.LogInformation("Initializing metadata database with migrations...");

            var connectionString = _metadataContext.Database.GetConnectionString();
            var databaseName = ExtractDatabaseNameFromConnectionString(connectionString ?? throw new InvalidOperationException("Metadata database connection string is null"));
            
            EnsureDatabaseExists(databaseName, connectionString);
            _metadataContext.Database.Migrate();

            _logger.LogInformation("Metadata database migrations completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing metadata database");
            throw;
        }
    }

    private void InitializeStorageDatabase()
    {
        try
        {
            _logger.LogInformation("Initializing storage database with migrations...");

            var connectionString = _storageContext.Database.GetConnectionString();
            var databaseName = ExtractDatabaseNameFromConnectionString(connectionString ?? throw new InvalidOperationException("Storage database connection string is null"));
            
            EnsureDatabaseExists(databaseName, connectionString ?? throw new InvalidOperationException("Storage database connection string is null"));
            _storageContext.Database.Migrate();

            _logger.LogInformation("Storage database migrations completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing storage database");
            throw;
        }
    }

    private string ExtractDatabaseNameFromConnectionString(string connectionString)
    {
        var builder = new NpgsqlConnectionStringBuilder(connectionString);
        return builder.Database ?? throw new InvalidOperationException("Database name not found in connection string");
    }

    private void EnsureDatabaseExists(string databaseName, string originalConnectionString)
    {
        try
        {
            var builder = new NpgsqlConnectionStringBuilder(originalConnectionString);
            builder.Database = "postgres";
            
            var masterConnectionString = builder.ConnectionString;

            using var connection = new NpgsqlConnection(masterConnectionString);
            connection.Open();

            var checkDbQuery = $"SELECT 1 FROM pg_database WHERE datname = '{databaseName}'";
            using var checkCommand = new NpgsqlCommand(checkDbQuery, connection);
            var exists = checkCommand.ExecuteScalar() != null;

            if (!exists)
            {
                _logger.LogInformation("Database '{DatabaseName}' does not exist, creating...", databaseName);
                
                var createDbQuery = $"CREATE DATABASE \"{databaseName}\"";
                using var createCommand = new NpgsqlCommand(createDbQuery, connection);
                createCommand.ExecuteNonQuery();
                
                _logger.LogInformation("Database '{DatabaseName}' created successfully", databaseName);
            }
            else
            {
                _logger.LogDebug("Database '{DatabaseName}' already exists", databaseName);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error ensuring database '{DatabaseName}' exists", databaseName);
            throw;
        }
    }

    private void InitializeMongoDBDatabases()
    {
        try
        {
            var connectionString = _configuration.GetConnectionString("MongoDB");
            if (!string.IsNullOrEmpty(connectionString))
            {
                var client = new MongoClient(connectionString);
                client.ListDatabaseNames();
                _logger.LogInformation("MongoDB connection verified successfully");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error connecting to MongoDB");
            throw;
        }
    }
}

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using FileChunkingSystem.Infrastructure.Data;
using MongoDB.Driver;
using Npgsql;

namespace FileChunkingSystem.Infrastructure.Services;

/// <summary>
/// Interface for database initialization operations
/// </summary>
public interface IDatabaseInitializer
{
    /// <summary>
    /// Initializes all required databases and applies migrations
    /// </summary>
    void Initialize();
}

/// <summary>
/// Service responsible for initializing and migrating all databases used by the application.
/// Handles PostgreSQL databases for metadata and storage, and verifies MongoDB connectivity.
/// </summary>
public class DatabaseInitializer : IDatabaseInitializer
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<DatabaseInitializer> _logger;
    private readonly MetadataDbContext _metadataContext;
    private readonly StorageDbContext _storageContext;

    /// <summary>
    /// Initializes a new instance of the DatabaseInitializer
    /// </summary>
    /// <param name="configuration">Application configuration</param>
    /// <param name="logger">Logger instance</param>
    /// <param name="metadataContext">Metadata database context</param>
    /// <param name="storageContext">Storage database context</param>
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

    /// <summary>
    /// Initializes all databases and applies necessary migrations
    /// </summary>
    public void Initialize()
    {
        InitializeMetadataDatabase();
        InitializeStorageDatabase();
        InitializeMongoDBDatabases();
    }

    /// <summary>
    /// Initializes the metadata database and applies EF Core migrations
    /// </summary>
    private void InitializeMetadataDatabase()
    {
        try
        {
            _logger.LogInformation("Initializing metadata database with migrations...");

            var connectionString = _metadataContext.Database.GetConnectionString();
            var databaseName = ExtractDatabaseNameFromConnectionString(connectionString ?? 
                throw new InvalidOperationException("Metadata database connection string is null"));
            
            // Ensure database exists before applying migrations
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

    /// <summary>
    /// Initializes the storage database and applies EF Core migrations
    /// </summary>
    private void InitializeStorageDatabase()
    {
        try
        {
            _logger.LogInformation("Initializing storage database with migrations...");

            var connectionString = _storageContext.Database.GetConnectionString();
            var databaseName = ExtractDatabaseNameFromConnectionString(connectionString ?? 
                throw new InvalidOperationException("Storage database connection string is null"));
            
            // Ensure database exists before applying migrations
            EnsureDatabaseExists(databaseName, connectionString ?? 
                throw new InvalidOperationException("Storage database connection string is null"));
            _storageContext.Database.Migrate();

            _logger.LogInformation("Storage database migrations completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing storage database");
            throw;
        }
    }

    /// <summary>
    /// Extracts database name from PostgreSQL connection string
    /// </summary>
    /// <param name="connectionString">The connection string</param>
    /// <returns>Database name</returns>
    private string ExtractDatabaseNameFromConnectionString(string connectionString)
    {
        var builder = new NpgsqlConnectionStringBuilder(connectionString);
        return builder.Database ?? throw new InvalidOperationException("Database name not found in connection string");
    }

    /// <summary>
    /// Ensures that the specified PostgreSQL database exists, creates it if not
    /// </summary>
    /// <param name="databaseName">Name of the database</param>
    /// <param name="originalConnectionString">Original connection string</param>
    private void EnsureDatabaseExists(string databaseName, string originalConnectionString)
    {
        try
        {
            // Connect to postgres master database to check/create target database
            var builder = new NpgsqlConnectionStringBuilder(originalConnectionString);
            builder.Database = "postgres";
            
            var masterConnectionString = builder.ConnectionString;

            using var connection = new NpgsqlConnection(masterConnectionString);
            connection.Open();

            // Check if database exists
            var checkDbQuery = $"SELECT 1 FROM pg_database WHERE datname = '{databaseName}'";
            using var checkCommand = new NpgsqlCommand(checkDbQuery, connection);
            var exists = checkCommand.ExecuteScalar() != null;

            if (!exists)
            {
                _logger.LogInformation("Database '{DatabaseName}' does not exist, creating...", databaseName);
                
                // Create database
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

    /// <summary>
    /// Verifies MongoDB connectivity and database accessibility
    /// </summary>
    private void InitializeMongoDBDatabases()
    {
        try
        {
            var connectionString = _configuration.GetConnectionString("MongoDB");
            if (!string.IsNullOrEmpty(connectionString))
            {
                var client = new MongoClient(connectionString);
                // Test connection by listing databases
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

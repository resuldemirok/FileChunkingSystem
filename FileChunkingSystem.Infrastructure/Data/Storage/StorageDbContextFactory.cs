using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.EnvironmentVariables;
using System.IO;

namespace FileChunkingSystem.Infrastructure.Data
{
    /// <summary>
    /// Design-time factory for creating StorageDbContext instances.
    /// Used by Entity Framework Core tools for migrations and database operations.
    /// </summary>
    public class StorageDbContextFactory : IDesignTimeDbContextFactory<StorageDbContext>
    {
        /// <summary>
        /// Creates a new instance of StorageDbContext for design-time operations
        /// </summary>
        /// <param name="args">Command line arguments</param>
        /// <returns>Configured StorageDbContext instance</returns>
        public StorageDbContext CreateDbContext(string[] args)
        {
            // Build configuration from appsettings and environment variables
            var basePath = Directory.GetCurrentDirectory();
            var configuration = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            // Configure DbContext options with PostgreSQL connection
            var optionsBuilder = new DbContextOptionsBuilder<StorageDbContext>();
            var connectionString = configuration.GetConnectionString("PostgreSQL");
            
            optionsBuilder.UseNpgsql(connectionString);

            return new StorageDbContext(optionsBuilder.Options);
        }
    }
}

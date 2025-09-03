using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.EnvironmentVariables;
using System.IO;

namespace FileChunkingSystem.Infrastructure.Data
{
    /// <summary>
    /// Design-time factory for creating MetadataDbContext instances.
    /// Used by Entity Framework Core tools for migrations and database operations.
    /// </summary>
    public class MetadataDbContextFactory : IDesignTimeDbContextFactory<MetadataDbContext>
    {
        /// <summary>
        /// Creates a new instance of MetadataDbContext for design-time operations
        /// </summary>
        /// <param name="args">Command line arguments</param>
        /// <returns>Configured MetadataDbContext instance</returns>
        public MetadataDbContext CreateDbContext(string[] args)
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
            var optionsBuilder = new DbContextOptionsBuilder<MetadataDbContext>();
            var connectionString = configuration.GetConnectionString("MetadataConnection");
            
            optionsBuilder.UseNpgsql(connectionString);

            return new MetadataDbContext(optionsBuilder.Options);
        }
    }
}

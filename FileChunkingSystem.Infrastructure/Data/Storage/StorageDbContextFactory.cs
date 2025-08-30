using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.EnvironmentVariables;
using System.IO;

namespace FileChunkingSystem.Infrastructure.Data
{
    public class StorageDbContextFactory : IDesignTimeDbContextFactory<StorageDbContext>
    {
        public StorageDbContext CreateDbContext(string[] args)
        {
            var basePath = Directory.GetCurrentDirectory();
            var configuration = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<StorageDbContext>();
            var connectionString = configuration.GetConnectionString("PostgreSQL");
            optionsBuilder.UseNpgsql(connectionString);

            return new StorageDbContext(optionsBuilder.Options);
        }
    }
}

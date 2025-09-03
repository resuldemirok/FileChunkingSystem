using FileChunkingSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FileChunkingSystem.Infrastructure.Data;

/// <summary>
/// Database context for PostgreSQL chunk storage.
/// Manages the physical storage of chunk data in PostgreSQL database.
/// </summary>
public class StorageDbContext : DbContext
{
    public StorageDbContext(DbContextOptions<StorageDbContext> options) : base(options) { }

    /// <summary>
    /// DbSet for chunk storage data in PostgreSQL
    /// </summary>
    public DbSet<ChunkStorage> ChunkStorage { get; set; }

    /// <summary>
    /// Configures entity mappings for the storage database
    /// </summary>
    /// <param name="modelBuilder">The model builder instance</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure ChunkStorage entity for PostgreSQL storage
        modelBuilder.Entity<ChunkStorage>(entity =>
        {
            entity.ToTable("chunk_storage", "storage");

            entity.HasKey(e => e.Key); // Storage key as primary key
            entity.Property(e => e.Key).HasMaxLength(1000);
            entity.Property(e => e.GroupName).HasMaxLength(1000);
            entity.Property(e => e.Data).IsRequired(); // Binary data storage
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            
            entity.HasIndex(e => new { e.GroupName, e.Key });
            entity.HasIndex(e => e.CreatedAt);
        });
    }
}

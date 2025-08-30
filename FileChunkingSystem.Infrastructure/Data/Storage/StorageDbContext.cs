using FileChunkingSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FileChunkingSystem.Infrastructure.Data;

public class StorageDbContext : DbContext
{
    public StorageDbContext(DbContextOptions<StorageDbContext> options) : base(options) { }

    public DbSet<ChunkStorage> ChunkStorage { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ChunkStorage>(entity =>
        {
            entity.ToTable("chunk_storage", "storage");

            entity.HasKey(e => e.Key);
            entity.Property(e => e.Key).HasMaxLength(1000);
            entity.Property(e => e.GroupName).HasMaxLength(1000);
            entity.Property(e => e.Data).IsRequired();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            
            entity.HasIndex(e => new { e.GroupName, e.Key });
            entity.HasIndex(e => e.CreatedAt);
        });
    }
}

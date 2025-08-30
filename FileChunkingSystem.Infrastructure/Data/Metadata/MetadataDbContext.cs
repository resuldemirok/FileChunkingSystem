using FileChunkingSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FileChunkingSystem.Infrastructure.Data;

public class MetadataDbContext : DbContext
{
    public MetadataDbContext(DbContextOptions<MetadataDbContext> options) : base(options) { }

    public DbSet<ChunkMetadata> ChunkMetadata { get; set; }
    public DbSet<ErrorLog> ErrorLogs { get; set; }
    public DbSet<FileMetadata> FileMetadata { get; set; }
    public DbSet<PerformanceLog> PerformanceLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<FileMetadata>(entity =>
        {
            entity.ToTable("file_metadata", "metadata");

            entity.HasKey(e => e.Id);
            entity.Property(e => e.OriginalFileName).IsRequired().HasMaxLength(500);
            entity.Property(e => e.FileExtension).HasMaxLength(50);
            entity.Property(e => e.ChecksumSha256).IsRequired().HasMaxLength(64);
            entity.HasIndex(e => e.ChecksumSha256);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        modelBuilder.Entity<ChunkMetadata>(entity =>
        {
            entity.ToTable("chunk_metadata", "metadata");

            entity.HasKey(e => e.Id);
            entity.Property(e => e.ChecksumSha256).IsRequired().HasMaxLength(64);
            entity.Property(e => e.StorageKey).IsRequired().HasMaxLength(1000);
            entity.HasOne(e => e.FileMetadata)
                  .WithMany(e => e.Chunks)
                  .HasForeignKey(e => e.FileMetadataId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => new { e.FileMetadataId, e.ChunkIndex }).IsUnique();
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        modelBuilder.Entity<ErrorLog>(entity =>
        {
            entity.ToTable("error_log", "log");

            entity.HasKey(e => e.Id);
            entity.Property(e => e.Message).IsRequired();
            entity.Property(e => e.Source).HasMaxLength(500);
            entity.HasIndex(e => e.Timestamp);
        });

        modelBuilder.Entity<PerformanceLog>(entity =>
        {
            entity.ToTable("performance_log", "log");

            entity.HasKey(e => e.Id);
            entity.Property(e => e.OperationType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.ErrorMessage).HasMaxLength(1000);
            entity.Property(e => e.AdditionalData);
            
            entity.HasOne(e => e.FileMetadata)
                .WithMany()
                .HasForeignKey(e => e.FileMetadataId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }
}

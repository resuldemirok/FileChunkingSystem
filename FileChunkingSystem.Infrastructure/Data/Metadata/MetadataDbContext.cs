using FileChunkingSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FileChunkingSystem.Infrastructure.Data;

/// <summary>
/// Database context for managing file and chunk metadata information.
/// Handles entities like FileMetadata, ChunkMetadata, ErrorLog, and PerformanceLog.
/// </summary>
public class MetadataDbContext : DbContext
{
    public MetadataDbContext(DbContextOptions<MetadataDbContext> options) : base(options) { }

    /// <summary>
    /// DbSet for chunk metadata information
    /// </summary>
    public DbSet<ChunkMetadata> ChunkMetadata { get; set; }
    
    /// <summary>
    /// DbSet for error logging
    /// </summary>
    public DbSet<ErrorLog> ErrorLogs { get; set; }
    
    /// <summary>
    /// DbSet for file metadata information
    /// </summary>
    public DbSet<FileMetadata> FileMetadata { get; set; }
    
    /// <summary>
    /// DbSet for performance logging
    /// </summary>
    public DbSet<PerformanceLog> PerformanceLogs { get; set; }

    /// <summary>
    /// Configures entity mappings and relationships for the metadata database
    /// </summary>
    /// <param name="modelBuilder">The model builder instance</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure FileMetadata entity
        modelBuilder.Entity<FileMetadata>(entity =>
        {
            entity.ToTable("file_metadata", "metadata");

            entity.HasKey(e => e.Id);
            entity.Property(e => e.OriginalFileName).IsRequired().HasMaxLength(500);
            entity.Property(e => e.FileExtension).HasMaxLength(50);
            entity.Property(e => e.ChecksumSha256).IsRequired().HasMaxLength(64);
            entity.HasIndex(e => e.ChecksumSha256); // Index for fast checksum lookups
            entity.HasQueryFilter(e => !e.IsDeleted); // Global query filter for soft delete
        });

        // Configure ChunkMetadata entity
        modelBuilder.Entity<ChunkMetadata>(entity =>
        {
            entity.ToTable("chunk_metadata", "metadata");

            entity.HasKey(e => e.Id);
            entity.Property(e => e.ChecksumSha256).IsRequired().HasMaxLength(64);
            entity.Property(e => e.StorageKey).IsRequired().HasMaxLength(1000);
            
            // Configure relationship with FileMetadata
            entity.HasOne(e => e.FileMetadata)
                  .WithMany(e => e.Chunks)
                  .HasForeignKey(e => e.FileMetadataId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            // Unique index for file and chunk combination
            entity.HasIndex(e => new { e.FileMetadataId, e.ChunkIndex }).IsUnique();
            entity.HasQueryFilter(e => !e.IsDeleted); // Global query filter for soft delete
        });

        // Configure ErrorLog entity
        modelBuilder.Entity<ErrorLog>(entity =>
        {
            entity.ToTable("error_log", "log");

            entity.HasKey(e => e.Id);
            entity.Property(e => e.Message).IsRequired();
            entity.Property(e => e.Source).HasMaxLength(500);
            entity.HasIndex(e => e.Timestamp); // Index for time-based queries
        });

        // Configure PerformanceLog entity
        modelBuilder.Entity<PerformanceLog>(entity =>
        {
            entity.ToTable("performance_log", "log");

            entity.HasKey(e => e.Id);
            entity.Property(e => e.OperationType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.ErrorMessage).HasMaxLength(1000);
            entity.Property(e => e.AdditionalData);
            
            // Optional relationship with FileMetadata
            entity.HasOne(e => e.FileMetadata)
                .WithMany()
                .HasForeignKey(e => e.FileMetadataId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }
}

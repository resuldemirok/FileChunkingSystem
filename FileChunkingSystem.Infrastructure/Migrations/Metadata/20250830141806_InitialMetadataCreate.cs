using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FileChunkingSystem.Infrastructure.Migrations.Metadata
{
    /// <inheritdoc />
    public partial class InitialMetadataCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "metadata");

            migrationBuilder.EnsureSchema(
                name: "log");

            migrationBuilder.CreateTable(
                name: "error_log",
                schema: "log",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Message = table.Column<string>(type: "text", nullable: false),
                    StackTrace = table.Column<string>(type: "text", nullable: false),
                    Source = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_error_log", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "file_metadata",
                schema: "metadata",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OriginalFileName = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    FileExtension = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    ChecksumSha256 = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ChunkingAlgorithm = table.Column<int>(type: "integer", nullable: false),
                    TotalChunks = table.Column<int>(type: "integer", nullable: false),
                    ChunkSize = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_file_metadata", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "chunk_metadata",
                schema: "metadata",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FileMetadataId = table.Column<Guid>(type: "uuid", nullable: false),
                    ChunkIndex = table.Column<int>(type: "integer", nullable: false),
                    ChunkSize = table.Column<int>(type: "integer", nullable: false),
                    ChecksumSha256 = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    StorageProviderType = table.Column<int>(type: "integer", nullable: false),
                    StorageKey = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    ChunkData = table.Column<byte[]>(type: "bytea", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_chunk_metadata", x => x.Id);
                    table.ForeignKey(
                        name: "FK_chunk_metadata_file_metadata_FileMetadataId",
                        column: x => x.FileMetadataId,
                        principalSchema: "metadata",
                        principalTable: "file_metadata",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "performance_log",
                schema: "log",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FileMetadataId = table.Column<Guid>(type: "uuid", nullable: true),
                    OperationType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DurationMs = table.Column<double>(type: "double precision", nullable: false),
                    BytesProcessed = table.Column<long>(type: "bigint", nullable: false),
                    ChunksProcessed = table.Column<int>(type: "integer", nullable: false),
                    ErrorMessage = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    AdditionalData = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_performance_log", x => x.Id);
                    table.ForeignKey(
                        name: "FK_performance_log_file_metadata_FileMetadataId",
                        column: x => x.FileMetadataId,
                        principalSchema: "metadata",
                        principalTable: "file_metadata",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_chunk_metadata_FileMetadataId_ChunkIndex",
                schema: "metadata",
                table: "chunk_metadata",
                columns: new[] { "FileMetadataId", "ChunkIndex" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_error_log_Timestamp",
                schema: "log",
                table: "error_log",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_file_metadata_ChecksumSha256",
                schema: "metadata",
                table: "file_metadata",
                column: "ChecksumSha256");

            migrationBuilder.CreateIndex(
                name: "IX_performance_log_FileMetadataId",
                schema: "log",
                table: "performance_log",
                column: "FileMetadataId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "chunk_metadata",
                schema: "metadata");

            migrationBuilder.DropTable(
                name: "error_log",
                schema: "log");

            migrationBuilder.DropTable(
                name: "performance_log",
                schema: "log");

            migrationBuilder.DropTable(
                name: "file_metadata",
                schema: "metadata");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FileChunkingSystem.Infrastructure.Migrations.Storage
{
    /// <inheritdoc />
    public partial class InitialStorageCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "storage");

            migrationBuilder.CreateTable(
                name: "chunk_storage",
                schema: "storage",
                columns: table => new
                {
                    Key = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    GroupName = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Data = table.Column<byte[]>(type: "bytea", nullable: false),
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_chunk_storage", x => x.Key);
                });

            migrationBuilder.CreateIndex(
                name: "IX_chunk_storage_CreatedAt",
                schema: "storage",
                table: "chunk_storage",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_chunk_storage_GroupName_Key",
                schema: "storage",
                table: "chunk_storage",
                columns: new[] { "GroupName", "Key" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "chunk_storage",
                schema: "storage");
        }
    }
}

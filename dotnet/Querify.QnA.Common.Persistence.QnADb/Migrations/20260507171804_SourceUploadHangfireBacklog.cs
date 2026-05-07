using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Querify.QnA.Common.Persistence.QnADb.Migrations
{
    /// <inheritdoc />
    public partial class SourceUploadHangfireBacklog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "SizeBytes",
                table: "Sources",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StorageKey",
                table: "Sources",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UploadChecksum",
                table: "Sources",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UploadStatus",
                table: "Sources",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateIndex(
                name: "IX_Sources_TenantId_StorageKey",
                table: "Sources",
                columns: new[] { "TenantId", "StorageKey" },
                unique: true,
                filter: "\"StorageKey\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Sources_TenantId_StorageKey",
                table: "Sources");

            migrationBuilder.DropColumn(
                name: "SizeBytes",
                table: "Sources");

            migrationBuilder.DropColumn(
                name: "StorageKey",
                table: "Sources");

            migrationBuilder.DropColumn(
                name: "UploadChecksum",
                table: "Sources");

            migrationBuilder.DropColumn(
                name: "UploadStatus",
                table: "Sources");
        }
    }
}

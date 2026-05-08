using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Querify.QnA.Common.Persistence.QnADb.Migrations
{
    /// <inheritdoc />
    public partial class SourceKindAndUploadCkecksumRemoved : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Kind",
                table: "Sources");

            migrationBuilder.DropColumn(
                name: "UploadChecksum",
                table: "Sources");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Kind",
                table: "Sources",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "UploadChecksum",
                table: "Sources",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);
        }
    }
}

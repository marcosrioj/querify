using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Querify.QnA.Common.Persistence.QnADb.Migrations
{
    /// <inheritdoc />
    public partial class SourceVisibilityRemoved : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastVerifiedAtUtc",
                table: "Sources");

            migrationBuilder.DropColumn(
                name: "Visibility",
                table: "Sources");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastVerifiedAtUtc",
                table: "Sources",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Visibility",
                table: "Sources",
                type: "integer",
                nullable: false,
                defaultValue: 1);
        }
    }
}

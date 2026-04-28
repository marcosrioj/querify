using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BaseFaq.QnA.Common.Persistence.QnADb.Migrations
{
    /// <inheritdoc />
    public partial class SourceSpaceUpdated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PublishedAtUtc",
                table: "Spaces");

            migrationBuilder.DropColumn(
                name: "AllowsCitation",
                table: "Sources");

            migrationBuilder.DropColumn(
                name: "CapturedAtUtc",
                table: "Sources");

            migrationBuilder.DropColumn(
                name: "AnsweredAtUtc",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "ValidatedAtUtc",
                table: "Questions");

            migrationBuilder.RenameColumn(
                name: "Kind",
                table: "Spaces",
                newName: "Status");

            migrationBuilder.AlterColumn<string>(
                name: "ContextNote",
                table: "Sources",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(1000)",
                oldMaxLength: 1000,
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Status",
                table: "Spaces",
                newName: "Kind");

            migrationBuilder.AddColumn<DateTime>(
                name: "PublishedAtUtc",
                table: "Spaces",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ContextNote",
                table: "Sources",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(2000)",
                oldMaxLength: 2000,
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "AllowsCitation",
                table: "Sources",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "CapturedAtUtc",
                table: "Sources",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AnsweredAtUtc",
                table: "Questions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ValidatedAtUtc",
                table: "Questions",
                type: "timestamp with time zone",
                nullable: true);
        }
    }
}

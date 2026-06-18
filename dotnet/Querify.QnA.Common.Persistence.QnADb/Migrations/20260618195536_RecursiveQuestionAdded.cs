using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Querify.QnA.Common.Persistence.QnADb.Migrations
{
    /// <inheritdoc />
    public partial class RecursiveQuestionAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ParentAnswerId",
                table: "Questions",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Questions_ParentAnswerId",
                table: "Questions",
                column: "ParentAnswerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Questions_Answers_ParentAnswerId",
                table: "Questions",
                column: "ParentAnswerId",
                principalTable: "Answers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Questions_Answers_ParentAnswerId",
                table: "Questions");

            migrationBuilder.DropIndex(
                name: "IX_Questions_ParentAnswerId",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "ParentAnswerId",
                table: "Questions");
        }
    }
}

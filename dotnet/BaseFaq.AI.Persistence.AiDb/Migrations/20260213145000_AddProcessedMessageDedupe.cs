using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BaseFaq.AI.Persistence.AiDb.Migrations
{
    public partial class AddProcessedMessageDedupe : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProcessedMessages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    HandlerName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    MessageId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ProcessedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcessedMessages", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GenerationJob_FaqId_IdempotencyKey",
                table: "GenerationJobs",
                columns: new[] { "FaqId", "IdempotencyKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProcessedMessage_HandlerName_MessageId",
                table: "ProcessedMessages",
                columns: new[] { "HandlerName", "MessageId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProcessedMessage_IsDeleted",
                table: "ProcessedMessages",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessedMessage_ProcessedUtc",
                table: "ProcessedMessages",
                column: "ProcessedUtc");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProcessedMessages");

            migrationBuilder.DropIndex(
                name: "IX_GenerationJob_FaqId_IdempotencyKey",
                table: "GenerationJobs");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BaseFaq.Faq.Common.Persistence.FaqDb.Migrations
{
    /// <inheritdoc />
    public partial class FaqItemAnswerAndVoteAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Answer",
                table: "FaqItems");

            migrationBuilder.DropColumn(
                name: "ShortAnswer",
                table: "FaqItems");

            migrationBuilder.CreateTable(
                name: "FaqItemAnswers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ShortAnswer = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    Answer = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: true),
                    Sort = table.Column<int>(type: "integer", nullable: false),
                    VoteScore = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    FaqItemId = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_FaqItemAnswers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FaqItemAnswers_FaqItems_FaqItemId",
                        column: x => x.FaqItemId,
                        principalTable: "FaqItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Votes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserPrint = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    Ip = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    UserAgent = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    FaqItemAnswerId = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_Votes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Votes_FaqItemAnswers_FaqItemAnswerId",
                        column: x => x.FaqItemAnswerId,
                        principalTable: "FaqItemAnswers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FaqItemAnswer_FaqItemId",
                table: "FaqItemAnswers",
                column: "FaqItemId");

            migrationBuilder.CreateIndex(
                name: "IX_FaqItemAnswer_IsDeleted",
                table: "FaqItemAnswers",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_FaqItemAnswer_TenantId",
                table: "FaqItemAnswers",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Vote_FaqItemAnswerId",
                table: "Votes",
                column: "FaqItemAnswerId");

            migrationBuilder.CreateIndex(
                name: "IX_Vote_IsDeleted",
                table: "Votes",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Vote_TenantId",
                table: "Votes",
                column: "TenantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Votes");

            migrationBuilder.DropTable(
                name: "FaqItemAnswers");

            migrationBuilder.AddColumn<string>(
                name: "Answer",
                table: "FaqItems",
                type: "character varying(5000)",
                maxLength: 5000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShortAnswer",
                table: "FaqItems",
                type: "character varying(250)",
                maxLength: 250,
                nullable: false,
                defaultValue: "");
        }
    }
}

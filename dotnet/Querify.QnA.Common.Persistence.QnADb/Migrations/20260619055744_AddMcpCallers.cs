using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Querify.QnA.Common.Persistence.QnADb.Migrations
{
    /// <inheritdoc />
    public partial class AddMcpCallers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Role",
                table: "SpaceSources",
                type: "integer",
                nullable: false,
                defaultValue: 16);

            migrationBuilder.CreateTable(
                name: "SourceGenerationRuns",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedSpaceId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    FailureReason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Warning = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    SpaceName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    SpaceSlug = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: true),
                    Language = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Visibility = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    SpaceStatus = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    AcceptsQuestions = table.Column<bool>(type: "boolean", nullable: false),
                    AcceptsAnswers = table.Column<bool>(type: "boolean", nullable: false),
                    ExtractionGoal = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    MaxTopLevelQuestions = table.Column<int>(type: "integer", nullable: false),
                    MaxFollowUpDepth = table.Column<int>(type: "integer", nullable: false),
                    MaxAnswersPerQuestion = table.Column<int>(type: "integer", nullable: false),
                    IncludeFollowUpQuestions = table.Column<bool>(type: "boolean", nullable: false),
                    TagGenerationMode = table.Column<int>(type: "integer", nullable: false, defaultValue: 11),
                    SourceRole = table.Column<int>(type: "integer", nullable: false, defaultValue: 11),
                    RequireEveryAnswerToCiteSource = table.Column<bool>(type: "boolean", nullable: false),
                    ContentHint = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    RawOutputJson = table.Column<string>(type: "character varying(12000)", maxLength: 12000, nullable: true),
                    StartedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_SourceGenerationRuns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SourceGenerationRuns_Sources_SourceId",
                        column: x => x.SourceId,
                        principalTable: "Sources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SourceGenerationRuns_Spaces_CreatedSpaceId",
                        column: x => x.CreatedSpaceId,
                        principalTable: "Spaces",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_QuestionTag_TenantId_TagId_QuestionId",
                table: "QuestionTags",
                columns: new[] { "TenantId", "TagId", "QuestionId" });

            migrationBuilder.CreateIndex(
                name: "IX_Questions_TenantId_Visibility_Status_SpaceId",
                table: "Questions",
                columns: new[] { "TenantId", "Visibility", "Status", "SpaceId" });

            migrationBuilder.CreateIndex(
                name: "IX_Answers_TenantId_Visibility_Status_QuestionId",
                table: "Answers",
                columns: new[] { "TenantId", "Visibility", "Status", "QuestionId" });

            migrationBuilder.CreateIndex(
                name: "IX_SourceGenerationRun_IsDeleted",
                table: "SourceGenerationRuns",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_SourceGenerationRun_TenantId",
                table: "SourceGenerationRuns",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_SourceGenerationRuns_CreatedSpaceId",
                table: "SourceGenerationRuns",
                column: "CreatedSpaceId");

            migrationBuilder.CreateIndex(
                name: "IX_SourceGenerationRuns_SourceId",
                table: "SourceGenerationRuns",
                column: "SourceId");

            migrationBuilder.CreateIndex(
                name: "IX_SourceGenerationRuns_TenantId_SourceId_CreatedDate",
                table: "SourceGenerationRuns",
                columns: new[] { "TenantId", "SourceId", "CreatedDate" });

            migrationBuilder.CreateIndex(
                name: "IX_SourceGenerationRuns_TenantId_Status",
                table: "SourceGenerationRuns",
                columns: new[] { "TenantId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SourceGenerationRuns");

            migrationBuilder.DropIndex(
                name: "IX_QuestionTag_TenantId_TagId_QuestionId",
                table: "QuestionTags");

            migrationBuilder.DropIndex(
                name: "IX_Questions_TenantId_Visibility_Status_SpaceId",
                table: "Questions");

            migrationBuilder.DropIndex(
                name: "IX_Answers_TenantId_Visibility_Status_QuestionId",
                table: "Answers");

            migrationBuilder.DropColumn(
                name: "Role",
                table: "SpaceSources");
        }
    }
}

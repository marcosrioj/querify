using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BaseFaq.QnA.Common.Persistence.QnADb.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Sources",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Kind = table.Column<int>(type: "integer", nullable: false),
                    Locator = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Label = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Scope = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    SystemName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ExternalId = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    Language = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    MediaType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Checksum = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    MetadataJson = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: true),
                    Visibility = table.Column<int>(type: "integer", nullable: false),
                    AllowsPublicCitation = table.Column<bool>(type: "boolean", nullable: false),
                    AllowsPublicExcerpt = table.Column<bool>(type: "boolean", nullable: false),
                    IsAuthoritative = table.Column<bool>(type: "boolean", nullable: false),
                    CapturedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastVerifiedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_Sources", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Spaces",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Key = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    Summary = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    DefaultLanguage = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Kind = table.Column<int>(type: "integer", nullable: false),
                    Visibility = table.Column<int>(type: "integer", nullable: false),
                    ModerationPolicy = table.Column<int>(type: "integer", nullable: false),
                    SearchMarkupMode = table.Column<int>(type: "integer", nullable: false),
                    ProductScope = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    JourneyScope = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    AcceptsQuestions = table.Column<bool>(type: "boolean", nullable: false),
                    AcceptsAnswers = table.Column<bool>(type: "boolean", nullable: false),
                    RequiresQuestionReview = table.Column<bool>(type: "boolean", nullable: false),
                    RequiresAnswerReview = table.Column<bool>(type: "boolean", nullable: false),
                    PublishedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastValidatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_Spaces", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tags",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
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
                    table.PrimaryKey("PK_Tags", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SpaceSources",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    SpaceId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceId = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_SpaceSources", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SpaceSources_Sources_SourceId",
                        column: x => x.SourceId,
                        principalTable: "Sources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SpaceSources_Spaces_SpaceId",
                        column: x => x.SpaceId,
                        principalTable: "Spaces",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SpaceTags",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    SpaceId = table.Column<Guid>(type: "uuid", nullable: false),
                    TagId = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_SpaceTags", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SpaceTags_Spaces_SpaceId",
                        column: x => x.SpaceId,
                        principalTable: "Spaces",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SpaceTags_Tags_TagId",
                        column: x => x.TagId,
                        principalTable: "Tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Answers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Headline = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    Body = table.Column<string>(type: "character varying(6000)", maxLength: 6000, nullable: true),
                    Kind = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Visibility = table.Column<int>(type: "integer", nullable: false),
                    Language = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ContextKey = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ApplicabilityRulesJson = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    TrustNote = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    EvidenceSummary = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    AuthorLabel = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ConfidenceScore = table.Column<int>(type: "integer", nullable: false),
                    Rank = table.Column<int>(type: "integer", nullable: false),
                    RevisionNumber = table.Column<int>(type: "integer", nullable: false),
                    QuestionId = table.Column<Guid>(type: "uuid", nullable: false),
                    PublishedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ValidatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AcceptedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RetiredAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_Answers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AnswerSourceLinks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    AnswerId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Role = table.Column<int>(type: "integer", nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_AnswerSourceLinks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AnswerSourceLinks_Answers_AnswerId",
                        column: x => x.AnswerId,
                        principalTable: "Answers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AnswerSourceLinks_Sources_SourceId",
                        column: x => x.SourceId,
                        principalTable: "Sources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Questions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Key = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Summary = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ContextNote = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Kind = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Visibility = table.Column<int>(type: "integer", nullable: false),
                    OriginChannel = table.Column<int>(type: "integer", nullable: false),
                    Language = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ProductScope = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    JourneyScope = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    AudienceScope = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ContextKey = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    OriginUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    OriginReference = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    ThreadSummary = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    ConfidenceScore = table.Column<int>(type: "integer", nullable: false),
                    RevisionNumber = table.Column<int>(type: "integer", nullable: false),
                    SpaceId = table.Column<Guid>(type: "uuid", nullable: false),
                    AcceptedAnswerId = table.Column<Guid>(type: "uuid", nullable: true),
                    DuplicateOfQuestionId = table.Column<Guid>(type: "uuid", nullable: true),
                    AnsweredAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ResolvedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ValidatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastActivityAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_Questions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Questions_Answers_AcceptedAnswerId",
                        column: x => x.AcceptedAnswerId,
                        principalTable: "Answers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Questions_Spaces_SpaceId",
                        column: x => x.SpaceId,
                        principalTable: "Spaces",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Questions_Questions_DuplicateOfQuestionId",
                        column: x => x.DuplicateOfQuestionId,
                        principalTable: "Questions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "QuestionSourceLinks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuestionId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Role = table.Column<int>(type: "integer", nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_QuestionSourceLinks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuestionSourceLinks_Sources_SourceId",
                        column: x => x.SourceId,
                        principalTable: "Sources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_QuestionSourceLinks_Questions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "Questions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QuestionTags",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuestionId = table.Column<Guid>(type: "uuid", nullable: false),
                    TagId = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_QuestionTags", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuestionTags_Questions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "Questions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_QuestionTags_Tags_TagId",
                        column: x => x.TagId,
                        principalTable: "Tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Activities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuestionId = table.Column<Guid>(type: "uuid", nullable: false),
                    AnswerId = table.Column<Guid>(type: "uuid", nullable: true),
                    Kind = table.Column<int>(type: "integer", nullable: false),
                    ActorKind = table.Column<int>(type: "integer", nullable: false),
                    ActorLabel = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Notes = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    MetadataJson = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    OccurredAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("PK_Activities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Activities_Answers_AnswerId",
                        column: x => x.AnswerId,
                        principalTable: "Answers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Activities_Questions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "Questions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Answer_IsDeleted",
                table: "Answers",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Answer_TenantId",
                table: "Answers",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Answers_QuestionId",
                table: "Answers",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_AnswerSourceLink_AnswerId_SourceId_Role_Order",
                table: "AnswerSourceLinks",
                columns: new[] { "AnswerId", "SourceId", "Role", "Order" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AnswerSourceLink_IsDeleted",
                table: "AnswerSourceLinks",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_AnswerSourceLink_TenantId",
                table: "AnswerSourceLinks",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_AnswerSourceLinks_SourceId",
                table: "AnswerSourceLinks",
                column: "SourceId");

            migrationBuilder.CreateIndex(
                name: "IX_Source_IsDeleted",
                table: "Sources",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Source_TenantId",
                table: "Sources",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Question_IsDeleted",
                table: "Questions",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Question_TenantId",
                table: "Questions",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Question_TenantId_Key",
                table: "Questions",
                columns: new[] { "TenantId", "Key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Questions_AcceptedAnswerId",
                table: "Questions",
                column: "AcceptedAnswerId");

            migrationBuilder.CreateIndex(
                name: "IX_Questions_DuplicateOfQuestionId",
                table: "Questions",
                column: "DuplicateOfQuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_Questions_SpaceId",
                table: "Questions",
                column: "SpaceId");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionSourceLink_IsDeleted",
                table: "QuestionSourceLinks",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionSourceLink_QuestionId_SourceId_Role_Order",
                table: "QuestionSourceLinks",
                columns: new[] { "QuestionId", "SourceId", "Role", "Order" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QuestionSourceLink_TenantId",
                table: "QuestionSourceLinks",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionSourceLinks_SourceId",
                table: "QuestionSourceLinks",
                column: "SourceId");

            migrationBuilder.CreateIndex(
                name: "IX_Space_IsDeleted",
                table: "Spaces",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Space_TenantId",
                table: "Spaces",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Space_TenantId_Key",
                table: "Spaces",
                columns: new[] { "TenantId", "Key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SpaceSource_IsDeleted",
                table: "SpaceSources",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_SpaceSource_SpaceId_SourceId",
                table: "SpaceSources",
                columns: new[] { "SpaceId", "SourceId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SpaceSource_TenantId",
                table: "SpaceSources",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_SpaceSources_SourceId",
                table: "SpaceSources",
                column: "SourceId");

            migrationBuilder.CreateIndex(
                name: "IX_SpaceTag_IsDeleted",
                table: "SpaceTags",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_SpaceTag_SpaceId_TagId",
                table: "SpaceTags",
                columns: new[] { "SpaceId", "TagId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SpaceTag_TenantId",
                table: "SpaceTags",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_SpaceTags_TagId",
                table: "SpaceTags",
                column: "TagId");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionTag_IsDeleted",
                table: "QuestionTags",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionTag_QuestionId_TagId",
                table: "QuestionTags",
                columns: new[] { "QuestionId", "TagId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QuestionTag_TenantId",
                table: "QuestionTags",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionTags_TagId",
                table: "QuestionTags",
                column: "TagId");

            migrationBuilder.CreateIndex(
                name: "IX_Activities_AnswerId",
                table: "Activities",
                column: "AnswerId");

            migrationBuilder.CreateIndex(
                name: "IX_Activity_IsDeleted",
                table: "Activities",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Activity_QuestionId_OccurredAtUtc",
                table: "Activities",
                columns: new[] { "QuestionId", "OccurredAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_Activity_TenantId",
                table: "Activities",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Tag_IsDeleted",
                table: "Tags",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Tag_TenantId",
                table: "Tags",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Tag_TenantId_Name",
                table: "Tags",
                columns: new[] { "TenantId", "Name" });

            migrationBuilder.AddForeignKey(
                name: "FK_Answers_Questions_QuestionId",
                table: "Answers",
                column: "QuestionId",
                principalTable: "Questions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Answers_Questions_QuestionId",
                table: "Answers");

            migrationBuilder.DropTable(
                name: "AnswerSourceLinks");

            migrationBuilder.DropTable(
                name: "QuestionSourceLinks");

            migrationBuilder.DropTable(
                name: "SpaceSources");

            migrationBuilder.DropTable(
                name: "SpaceTags");

            migrationBuilder.DropTable(
                name: "QuestionTags");

            migrationBuilder.DropTable(
                name: "Activities");

            migrationBuilder.DropTable(
                name: "Sources");

            migrationBuilder.DropTable(
                name: "Tags");

            migrationBuilder.DropTable(
                name: "Questions");

            migrationBuilder.DropTable(
                name: "Answers");

            migrationBuilder.DropTable(
                name: "Spaces");
        }
    }
}

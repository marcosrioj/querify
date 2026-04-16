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
                name: "KnowledgeSources",
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
                    table.PrimaryKey("PK_KnowledgeSources", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "QuestionSpaces",
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
                    table.PrimaryKey("PK_QuestionSpaces", x => x.Id);
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
                name: "QuestionSpaceSources",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuestionSpaceId = table.Column<Guid>(type: "uuid", nullable: false),
                    KnowledgeSourceId = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_QuestionSpaceSources", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuestionSpaceSources_KnowledgeSources_KnowledgeSourceId",
                        column: x => x.KnowledgeSourceId,
                        principalTable: "KnowledgeSources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_QuestionSpaceSources_QuestionSpaces_QuestionSpaceId",
                        column: x => x.QuestionSpaceId,
                        principalTable: "QuestionSpaces",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QuestionSpaceTags",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuestionSpaceId = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_QuestionSpaceTags", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuestionSpaceTags_QuestionSpaces_QuestionSpaceId",
                        column: x => x.QuestionSpaceId,
                        principalTable: "QuestionSpaces",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_QuestionSpaceTags_Tags_TagId",
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
                        name: "FK_AnswerSourceLinks_KnowledgeSources_SourceId",
                        column: x => x.SourceId,
                        principalTable: "KnowledgeSources",
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
                        name: "FK_Questions_QuestionSpaces_SpaceId",
                        column: x => x.SpaceId,
                        principalTable: "QuestionSpaces",
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
                        name: "FK_QuestionSourceLinks_KnowledgeSources_SourceId",
                        column: x => x.SourceId,
                        principalTable: "KnowledgeSources",
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
                name: "ThreadActivities",
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
                    table.PrimaryKey("PK_ThreadActivities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ThreadActivities_Answers_AnswerId",
                        column: x => x.AnswerId,
                        principalTable: "Answers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ThreadActivities_Questions_QuestionId",
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
                name: "IX_KnowledgeSource_IsDeleted",
                table: "KnowledgeSources",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_KnowledgeSource_TenantId",
                table: "KnowledgeSources",
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
                name: "IX_QuestionSpace_IsDeleted",
                table: "QuestionSpaces",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionSpace_TenantId",
                table: "QuestionSpaces",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionSpace_TenantId_Key",
                table: "QuestionSpaces",
                columns: new[] { "TenantId", "Key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QuestionSpaceSource_IsDeleted",
                table: "QuestionSpaceSources",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionSpaceSource_QuestionSpaceId_KnowledgeSourceId",
                table: "QuestionSpaceSources",
                columns: new[] { "QuestionSpaceId", "KnowledgeSourceId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QuestionSpaceSource_TenantId",
                table: "QuestionSpaceSources",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionSpaceSources_KnowledgeSourceId",
                table: "QuestionSpaceSources",
                column: "KnowledgeSourceId");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionSpaceTag_IsDeleted",
                table: "QuestionSpaceTags",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionSpaceTag_QuestionSpaceId_TagId",
                table: "QuestionSpaceTags",
                columns: new[] { "QuestionSpaceId", "TagId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QuestionSpaceTag_TenantId",
                table: "QuestionSpaceTags",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionSpaceTags_TagId",
                table: "QuestionSpaceTags",
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
                name: "IX_ThreadActivities_AnswerId",
                table: "ThreadActivities",
                column: "AnswerId");

            migrationBuilder.CreateIndex(
                name: "IX_ThreadActivity_IsDeleted",
                table: "ThreadActivities",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_ThreadActivity_QuestionId_OccurredAtUtc",
                table: "ThreadActivities",
                columns: new[] { "QuestionId", "OccurredAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_ThreadActivity_TenantId",
                table: "ThreadActivities",
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
                name: "QuestionSpaceSources");

            migrationBuilder.DropTable(
                name: "QuestionSpaceTags");

            migrationBuilder.DropTable(
                name: "QuestionTags");

            migrationBuilder.DropTable(
                name: "ThreadActivities");

            migrationBuilder.DropTable(
                name: "KnowledgeSources");

            migrationBuilder.DropTable(
                name: "Tags");

            migrationBuilder.DropTable(
                name: "Questions");

            migrationBuilder.DropTable(
                name: "Answers");

            migrationBuilder.DropTable(
                name: "QuestionSpaces");
        }
    }
}

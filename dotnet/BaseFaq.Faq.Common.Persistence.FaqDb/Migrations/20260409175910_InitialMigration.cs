using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BaseFaq.Faq.Common.Persistence.FaqDb.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ContentRefs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Kind = table.Column<int>(type: "integer", nullable: false),
                    Locator = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Label = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Scope = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("PK_ContentRefs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Faqs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Language = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_Faqs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tags",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Value = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
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
                    table.PrimaryKey("PK_Tags", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FaqContentRefs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    FaqId = table.Column<Guid>(type: "uuid", nullable: false),
                    ContentRefId = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_FaqContentRefs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FaqContentRefs_ContentRefs_ContentRefId",
                        column: x => x.ContentRefId,
                        principalTable: "ContentRefs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FaqContentRefs_Faqs_FaqId",
                        column: x => x.FaqId,
                        principalTable: "Faqs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FaqItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Question = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    ShortAnswer = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    Answer = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: true),
                    AdditionalInfo = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CtaTitle = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    CtaUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Sort = table.Column<int>(type: "integer", nullable: false),
                    FeedbackScore = table.Column<int>(type: "integer", nullable: false),
                    ConfidenceScore = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    FaqId = table.Column<Guid>(type: "uuid", nullable: false),
                    ContentRefId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_FaqItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FaqItems_ContentRefs_ContentRefId",
                        column: x => x.ContentRefId,
                        principalTable: "ContentRefs",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_FaqItems_Faqs_FaqId",
                        column: x => x.FaqId,
                        principalTable: "Faqs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FaqTags",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    FaqId = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_FaqTags", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FaqTags_Faqs_FaqId",
                        column: x => x.FaqId,
                        principalTable: "Faqs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FaqTags_Tags_TagId",
                        column: x => x.TagId,
                        principalTable: "Tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Feedbacks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Like = table.Column<bool>(type: "boolean", nullable: false),
                    UserPrint = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    Ip = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    UserAgent = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    UnLikeReason = table.Column<int>(type: "integer", nullable: true),
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
                    table.PrimaryKey("PK_Feedbacks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Feedbacks_FaqItems_FaqItemId",
                        column: x => x.FaqItemId,
                        principalTable: "FaqItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContentRef_IsDeleted",
                table: "ContentRefs",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_ContentRef_Kind",
                table: "ContentRefs",
                column: "Kind");

            migrationBuilder.CreateIndex(
                name: "IX_ContentRef_TenantId",
                table: "ContentRefs",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_FaqContentRef_FaqId_ContentRefId",
                table: "FaqContentRefs",
                columns: new[] { "FaqId", "ContentRefId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FaqContentRef_IsDeleted",
                table: "FaqContentRefs",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_FaqContentRef_TenantId",
                table: "FaqContentRefs",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_FaqContentRefs_ContentRefId",
                table: "FaqContentRefs",
                column: "ContentRefId");

            migrationBuilder.CreateIndex(
                name: "IX_FaqItem_FaqId",
                table: "FaqItems",
                column: "FaqId");

            migrationBuilder.CreateIndex(
                name: "IX_FaqItem_IsDeleted",
                table: "FaqItems",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_FaqItem_TenantId",
                table: "FaqItems",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_FaqItems_ContentRefId",
                table: "FaqItems",
                column: "ContentRefId");

            migrationBuilder.CreateIndex(
                name: "IX_Faq_IsDeleted",
                table: "Faqs",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Faq_TenantId",
                table: "Faqs",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_FaqTag_FaqId_TagId",
                table: "FaqTags",
                columns: new[] { "FaqId", "TagId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FaqTag_IsDeleted",
                table: "FaqTags",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_FaqTag_TenantId",
                table: "FaqTags",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_FaqTags_TagId",
                table: "FaqTags",
                column: "TagId");

            migrationBuilder.CreateIndex(
                name: "IX_Feedback_FaqItemId",
                table: "Feedbacks",
                column: "FaqItemId");

            migrationBuilder.CreateIndex(
                name: "IX_Feedback_IsDeleted",
                table: "Feedbacks",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Feedback_TenantId",
                table: "Feedbacks",
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
                name: "IX_Tag_Value",
                table: "Tags",
                column: "Value");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FaqContentRefs");

            migrationBuilder.DropTable(
                name: "FaqTags");

            migrationBuilder.DropTable(
                name: "Feedbacks");

            migrationBuilder.DropTable(
                name: "Tags");

            migrationBuilder.DropTable(
                name: "FaqItems");

            migrationBuilder.DropTable(
                name: "ContentRefs");

            migrationBuilder.DropTable(
                name: "Faqs");
        }
    }
}

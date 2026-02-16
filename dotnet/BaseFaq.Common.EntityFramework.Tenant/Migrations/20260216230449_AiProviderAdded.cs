using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BaseFaq.Common.EntityFramework.Tenant.Migrations
{
    /// <inheritdoc />
    public partial class AiProviderAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AiProviders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Provider = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Model = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Prompt = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: false),
                    Command = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_AiProviders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TenantAiProviders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    AiProviderId = table.Column<Guid>(type: "uuid", nullable: false),
                    AiProviderKey = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
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
                    table.PrimaryKey("PK_TenantAiProviders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TenantAiProviders_AiProviders_AiProviderId",
                        column: x => x.AiProviderId,
                        principalTable: "AiProviders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TenantAiProviders_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AiProvider_Command_Provider_Model",
                table: "AiProviders",
                columns: new[] { "Command", "Provider", "Model" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AiProvider_IsDeleted",
                table: "AiProviders",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_TenantAiProvider_IsDeleted",
                table: "TenantAiProviders",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_TenantAiProvider_TenantId_AiProviderId",
                table: "TenantAiProviders",
                columns: new[] { "TenantId", "AiProviderId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TenantAiProviders_AiProviderId",
                table: "TenantAiProviders",
                column: "AiProviderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TenantAiProviders");

            migrationBuilder.DropTable(
                name: "AiProviders");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BaseFaq.QnA.Common.Persistence.QnADb.Migrations
{
    /// <inheritdoc />
    public partial class ThreadActivityUserPrintAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserPrint",
                table: "ThreadActivities",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE "ThreadActivities"
                SET "UserPrint" = "ActorLabel"
                WHERE "UserPrint" IS NULL
                  AND "Kind" IN (14, 15)
                  AND "ActorLabel" IS NOT NULL
                  AND btrim("ActorLabel") <> '';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserPrint",
                table: "ThreadActivities");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BaseFaq.QnA.Common.Persistence.QnADb.Migrations
{
    /// <inheritdoc />
    public partial class ActivityUserPrintAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserPrint",
                table: "Activities",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Ip",
                table: "Activities",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UserAgent",
                table: "Activities",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql("""
                UPDATE "Activities"
                SET "UserPrint" = "ActorLabel"
                WHERE "UserPrint" IS NULL
                  AND "Kind" IN (14, 15)
                  AND "ActorLabel" IS NOT NULL
                  AND btrim("ActorLabel") <> '';
                """);

            migrationBuilder.Sql("""
                UPDATE "Activities"
                SET "UserPrint" = COALESCE(
                    NULLIF(btrim("UserPrint"), ''),
                    NULLIF(btrim("ActorLabel"), ''),
                    NULLIF(btrim("CreatedBy"), ''),
                    NULLIF(btrim("UpdatedBy"), ''),
                    'system')
                WHERE "UserPrint" IS NULL
                   OR btrim("UserPrint") = '';
                """);

            migrationBuilder.AlterColumn<string>(
                name: "UserPrint",
                table: "Activities",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200,
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "UserPrint",
                table: "Activities",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.DropColumn(
                name: "Ip",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "UserAgent",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "UserPrint",
                table: "Activities");
        }
    }
}

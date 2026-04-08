using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BaseFaq.Faq.Common.Persistence.FaqDb.Migrations
{
    /// <inheritdoc />
    public partial class FaqCtaAndFaqSortTypeRemoved : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CtaEnabled",
                table: "Faqs");

            migrationBuilder.DropColumn(
                name: "CtaTarget",
                table: "Faqs");

            migrationBuilder.DropColumn(
                name: "SortStrategy",
                table: "Faqs");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "CtaEnabled",
                table: "Faqs",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "CtaTarget",
                table: "Faqs",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SortStrategy",
                table: "Faqs",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}

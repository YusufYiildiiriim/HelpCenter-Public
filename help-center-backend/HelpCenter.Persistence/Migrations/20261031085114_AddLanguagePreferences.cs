using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HelpCenter.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddLanguagePreferences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DefaultLocale",
                table: "Companies",
                type: "character varying(16)",
                maxLength: 16,
                nullable: false,
                defaultValue: "tr-TR");

            migrationBuilder.AddColumn<string>(
                name: "PreferredLocale",
                table: "Accounts",
                type: "character varying(16)",
                maxLength: 16,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DefaultLocale",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "PreferredLocale",
                table: "Accounts");
        }
    }
}

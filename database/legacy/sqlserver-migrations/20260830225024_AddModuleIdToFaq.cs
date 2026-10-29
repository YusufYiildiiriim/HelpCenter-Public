using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HelpCenter.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddModuleIdToFaq : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FAQs_Projects_ProjectId",
                table: "FAQs");

            migrationBuilder.AddColumn<int>(
                name: "ModuleId",
                table: "FAQs",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_FAQs_ModuleId",
                table: "FAQs",
                column: "ModuleId");

            migrationBuilder.AddForeignKey(
                name: "FK_FAQs_Modules_ModuleId",
                table: "FAQs",
                column: "ModuleId",
                principalTable: "Modules",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FAQs_Projects_ProjectId",
                table: "FAQs",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FAQs_Modules_ModuleId",
                table: "FAQs");

            migrationBuilder.DropForeignKey(
                name: "FK_FAQs_Projects_ProjectId",
                table: "FAQs");

            migrationBuilder.DropIndex(
                name: "IX_FAQs_ModuleId",
                table: "FAQs");

            migrationBuilder.DropColumn(
                name: "ModuleId",
                table: "FAQs");

            migrationBuilder.AddForeignKey(
                name: "FK_FAQs_Projects_ProjectId",
                table: "FAQs",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id");
        }
    }
}

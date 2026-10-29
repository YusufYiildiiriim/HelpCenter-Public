using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HelpCenter.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProjectIdToReferenceEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProjectId",
                table: "RequestSubjects",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProjectId",
                table: "Guides",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProjectId",
                table: "FAQs",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProjectId",
                table: "CustomerRequestStatuses",
                type: "int",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "CustomerRequestStatuses",
                keyColumn: "Id",
                keyValue: 1,
                column: "ProjectId",
                value: null);

            migrationBuilder.UpdateData(
                table: "CustomerRequestStatuses",
                keyColumn: "Id",
                keyValue: 2,
                column: "ProjectId",
                value: null);

            migrationBuilder.UpdateData(
                table: "CustomerRequestStatuses",
                keyColumn: "Id",
                keyValue: 3,
                column: "ProjectId",
                value: null);

            migrationBuilder.UpdateData(
                table: "CustomerRequestStatuses",
                keyColumn: "Id",
                keyValue: 4,
                column: "ProjectId",
                value: null);

            migrationBuilder.CreateIndex(
                name: "IX_RequestSubjects_ProjectId",
                table: "RequestSubjects",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Guides_ProjectId",
                table: "Guides",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_FAQs_ProjectId",
                table: "FAQs",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerRequestStatuses_ProjectId",
                table: "CustomerRequestStatuses",
                column: "ProjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerRequestStatuses_Projects_ProjectId",
                table: "CustomerRequestStatuses",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_FAQs_Projects_ProjectId",
                table: "FAQs",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Guides_Projects_ProjectId",
                table: "Guides",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RequestSubjects_Projects_ProjectId",
                table: "RequestSubjects",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CustomerRequestStatuses_Projects_ProjectId",
                table: "CustomerRequestStatuses");

            migrationBuilder.DropForeignKey(
                name: "FK_FAQs_Projects_ProjectId",
                table: "FAQs");

            migrationBuilder.DropForeignKey(
                name: "FK_Guides_Projects_ProjectId",
                table: "Guides");

            migrationBuilder.DropForeignKey(
                name: "FK_RequestSubjects_Projects_ProjectId",
                table: "RequestSubjects");

            migrationBuilder.DropIndex(
                name: "IX_RequestSubjects_ProjectId",
                table: "RequestSubjects");

            migrationBuilder.DropIndex(
                name: "IX_Guides_ProjectId",
                table: "Guides");

            migrationBuilder.DropIndex(
                name: "IX_FAQs_ProjectId",
                table: "FAQs");

            migrationBuilder.DropIndex(
                name: "IX_CustomerRequestStatuses_ProjectId",
                table: "CustomerRequestStatuses");

            migrationBuilder.DropColumn(
                name: "ProjectId",
                table: "RequestSubjects");

            migrationBuilder.DropColumn(
                name: "ProjectId",
                table: "Guides");

            migrationBuilder.DropColumn(
                name: "ProjectId",
                table: "FAQs");

            migrationBuilder.DropColumn(
                name: "ProjectId",
                table: "CustomerRequestStatuses");
        }
    }
}

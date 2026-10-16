using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HelpCenter.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class LinkGuidesToModules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ModuleId",
                table: "Guides",
                type: "int",
                nullable: true);

            if (ActiveProvider == "Microsoft.EntityFrameworkCore.SqlServer")
            {
                migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM Guides g WHERE
    (SELECT COUNT(*) FROM Modules m WHERE m.Name COLLATE Latin1_General_100_BIN2 = g.Module COLLATE Latin1_General_100_BIN2
        AND DATALENGTH(m.Name) = DATALENGTH(g.Module)) <> 1)
    THROW 51000, 'Guide module mapping is missing or ambiguous. Reconcile names before migration.', 1;
UPDATE g SET ModuleId = m.Id FROM Guides g JOIN Modules m
    ON m.Name COLLATE Latin1_General_100_BIN2 = g.Module COLLATE Latin1_General_100_BIN2
    AND DATALENGTH(m.Name) = DATALENGTH(g.Module);");
            }

            migrationBuilder.CreateIndex(
                name: "IX_Guides_ModuleId",
                table: "Guides",
                column: "ModuleId");

            migrationBuilder.AddForeignKey(
                name: "FK_Guides_Modules_ModuleId",
                table: "Guides",
                column: "ModuleId",
                principalTable: "Modules",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            if (ActiveProvider == "Microsoft.EntityFrameworkCore.SqlServer")
                migrationBuilder.Sql("UPDATE g SET Module = m.Name FROM Guides g JOIN Modules m ON m.Id = g.ModuleId;");

            migrationBuilder.DropForeignKey(
                name: "FK_Guides_Modules_ModuleId",
                table: "Guides");

            migrationBuilder.DropIndex(
                name: "IX_Guides_ModuleId",
                table: "Guides");

            migrationBuilder.DropColumn(
                name: "ModuleId",
                table: "Guides");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HelpCenter.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveRoleIdFromDataRestrictions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DataRestrictions_Roles_RoleId",
                table: "DataRestrictions");

            migrationBuilder.DropForeignKey(
                name: "FK_DataRestrictions_Users_UserId",
                table: "DataRestrictions");

            migrationBuilder.DropIndex(
                name: "IX_DataRestrictions_RoleId",
                table: "DataRestrictions");

            migrationBuilder.DropColumn(
                name: "RoleId",
                table: "DataRestrictions");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "DataRestrictions",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_DataRestrictions_Users_UserId",
                table: "DataRestrictions",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DataRestrictions_Users_UserId",
                table: "DataRestrictions");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "DataRestrictions",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "RoleId",
                table: "DataRestrictions",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DataRestrictions_RoleId",
                table: "DataRestrictions",
                column: "RoleId");

            migrationBuilder.AddForeignKey(
                name: "FK_DataRestrictions_Roles_RoleId",
                table: "DataRestrictions",
                column: "RoleId",
                principalTable: "Roles",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DataRestrictions_Users_UserId",
                table: "DataRestrictions",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}

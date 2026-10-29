using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HelpCenter.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomerRefreshTokens : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "RefreshTokens",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "CustomerId",
                table: "RefreshTokens",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_CustomerId_ExpiresAt",
                table: "RefreshTokens",
                columns: new[] { "CustomerId", "ExpiresAt" });

            migrationBuilder.AddForeignKey(
                name: "FK_RefreshTokens_Customers_CustomerId",
                table: "RefreshTokens",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Customer-issued refresh tokens have no UserId and cannot survive the previous
            // user-only schema. Remove them before UserId becomes required again.
            migrationBuilder.Sql("DELETE FROM [RefreshTokens] WHERE [UserId] IS NULL;");

            migrationBuilder.DropForeignKey(
                name: "FK_RefreshTokens_Customers_CustomerId",
                table: "RefreshTokens");

            migrationBuilder.DropIndex(
                name: "IX_RefreshTokens_CustomerId_ExpiresAt",
                table: "RefreshTokens");

            migrationBuilder.DropColumn(
                name: "CustomerId",
                table: "RefreshTokens");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "RefreshTokens",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}

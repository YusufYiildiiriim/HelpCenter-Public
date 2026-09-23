using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HelpCenter.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAdminPassword : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Accounts",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$11$pCDJIRu1tH/Ir19q7P0fv.1oiPQ4SLP6vOEYU5LjinQtMzn5eL3f2");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Accounts",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$11$9bF3tUO21DrmzbGnpKIXKOUKwUG7J7JmVMPmDm5yHf1oREBn66Kse");
        }
    }
}

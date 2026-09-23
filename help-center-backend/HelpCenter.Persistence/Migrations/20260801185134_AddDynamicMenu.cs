using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace HelpCenter.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDynamicMenu : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MenuItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Label = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Icon = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Route = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ResourceKey = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ParentId = table.Column<int>(type: "int", nullable: true),
                    Order = table.Column<int>(type: "int", nullable: false),
                    GroupTitle = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedByAccountId = table.Column<int>(type: "int", nullable: true),
                    LastModifiedByAccountId = table.Column<int>(type: "int", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MenuItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MenuItems_MenuItems_ParentId",
                        column: x => x.ParentId,
                        principalTable: "MenuItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "MenuItems",
                columns: new[] { "Id", "CreatedAt", "CreatedByAccountId", "GroupTitle", "Icon", "IsActive", "IsDeleted", "Label", "LastModifiedByAccountId", "Order", "ParentId", "ResourceKey", "Route", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 8, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "Talep Takip", "BarChart3", true, false, "İstatistikler", null, 1, null, "Dashboard", "/admin", null },
                    { 2, new DateTime(2026, 8, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, "ClipboardList", true, false, "Talepler", null, 2, null, "Requests", "/admin/requests", null },
                    { 3, new DateTime(2026, 8, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, "Inbox", true, false, "Bana Atananlar", null, 3, null, "AssignedRequests", "/admin/requests/assigned", null },
                    { 4, new DateTime(2026, 8, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "Tanımlamalar", "Building2", true, false, "Firmalar", null, 4, null, "Companies", "/admin/companies", null },
                    { 5, new DateTime(2026, 8, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, "FolderKanban", true, false, "Projeler", null, 5, null, "Projects", "/admin/projects", null },
                    { 6, new DateTime(2026, 8, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, "Users", true, false, "Kullanıcılar", null, 6, null, "Users", "/admin/users", null },
                    { 7, new DateTime(2026, 8, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, "HelpCircle", true, false, "SSS Yönetimi", null, 7, null, "FAQ", "/admin/ss", null },
                    { 8, new DateTime(2026, 8, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, "BookOpen", true, false, "Kullanım Kılavuzu", null, 8, null, "Guide", "/admin/guide", null },
                    { 9, new DateTime(2026, 8, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, "Terminal", true, false, "Sistem Logları", null, 9, null, "Logs", "/admin/logs", null },
                    { 10, new DateTime(2026, 8, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "Sistem Ayarları", "ShieldCheck", true, false, "Rol ve Yetkiler", null, 10, null, "Roles", "/admin/settings/roles", null },
                    { 11, new DateTime(2026, 8, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, "LayoutGrid", true, false, "Departman Modülleri", null, 11, null, "Modules", "/admin/modules", null },
                    { 12, new DateTime(2026, 8, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, "MessageSquare", true, false, "Talep Konuları", null, 12, null, "Subjects", "/admin/subjects", null },
                    { 13, new DateTime(2026, 8, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, "LayoutList", true, false, "Talep Durumları", null, 13, null, "Statuses", "/admin/statuses", null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_MenuItems_ParentId",
                table: "MenuItems",
                column: "ParentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MenuItems");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HelpCenter.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddLookupContentTranslations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SourceLocale",
                table: "RequestSubjects",
                type: "character varying(16)",
                maxLength: 16,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SourceLocale",
                table: "Projects",
                type: "character varying(16)",
                maxLength: 16,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SourceLocale",
                table: "Modules",
                type: "character varying(16)",
                maxLength: 16,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ModuleTranslations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ModuleId = table.Column<Guid>(type: "uuid", nullable: false),
                    Locale = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    Name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedByAccountId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModifiedByAccountId = table.Column<Guid>(type: "uuid", nullable: true),
                    RowVersion = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModuleTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ModuleTranslations_Modules_ModuleId",
                        column: x => x.ModuleId,
                        principalTable: "Modules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProjectTranslations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    Locale = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    Name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedByAccountId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModifiedByAccountId = table.Column<Guid>(type: "uuid", nullable: true),
                    RowVersion = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectTranslations_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RequestSubjectTranslations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestSubjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    Locale = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    Name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedByAccountId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModifiedByAccountId = table.Column<Guid>(type: "uuid", nullable: true),
                    RowVersion = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequestSubjectTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RequestSubjectTranslations_RequestSubjects_RequestSubjectId",
                        column: x => x.RequestSubjectId,
                        principalTable: "RequestSubjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ModuleTranslations_ModuleId_Locale",
                table: "ModuleTranslations",
                columns: new[] { "ModuleId", "Locale" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ModuleTranslations_PublicId",
                table: "ModuleTranslations",
                column: "PublicId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectTranslations_ProjectId_Locale",
                table: "ProjectTranslations",
                columns: new[] { "ProjectId", "Locale" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectTranslations_PublicId",
                table: "ProjectTranslations",
                column: "PublicId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RequestSubjectTranslations_PublicId",
                table: "RequestSubjectTranslations",
                column: "PublicId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RequestSubjectTranslations_RequestSubjectId_Locale",
                table: "RequestSubjectTranslations",
                columns: new[] { "RequestSubjectId", "Locale" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ModuleTranslations");

            migrationBuilder.DropTable(
                name: "ProjectTranslations");

            migrationBuilder.DropTable(
                name: "RequestSubjectTranslations");

            migrationBuilder.DropColumn(
                name: "SourceLocale",
                table: "RequestSubjects");

            migrationBuilder.DropColumn(
                name: "SourceLocale",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "SourceLocale",
                table: "Modules");
        }
    }
}

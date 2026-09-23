using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HelpCenter.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPublicIdToBaseEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PublicId",
                table: "Users",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWID()");

            migrationBuilder.AddColumn<Guid>(
                name: "PublicId",
                table: "UserRoles",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWID()");

            migrationBuilder.AddColumn<Guid>(
                name: "PublicId",
                table: "UserProjects",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWID()");

            migrationBuilder.AddColumn<Guid>(
                name: "PublicId",
                table: "Roles",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWID()");

            migrationBuilder.AddColumn<Guid>(
                name: "PublicId",
                table: "RolePermissions",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWID()");

            migrationBuilder.AddColumn<Guid>(
                name: "PublicId",
                table: "RolePermissionActions",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWID()");

            migrationBuilder.AddColumn<Guid>(
                name: "PublicId",
                table: "RequestSubjects",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWID()");

            migrationBuilder.AddColumn<Guid>(
                name: "PublicId",
                table: "RequestHistories",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWID()");

            migrationBuilder.AddColumn<Guid>(
                name: "PublicId",
                table: "Projects",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWID()");

            migrationBuilder.AddColumn<Guid>(
                name: "PublicId",
                table: "ProjectModule",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWID()");

            migrationBuilder.AddColumn<Guid>(
                name: "PublicId",
                table: "OrganizationInfos",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWID()");

            migrationBuilder.AddColumn<Guid>(
                name: "PublicId",
                table: "Modules",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWID()");

            migrationBuilder.AddColumn<Guid>(
                name: "PublicId",
                table: "ModuleExperts",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWID()");

            migrationBuilder.AddColumn<Guid>(
                name: "PublicId",
                table: "MenuItems",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWID()");

            migrationBuilder.AddColumn<Guid>(
                name: "PublicId",
                table: "Guides",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWID()");

            migrationBuilder.AddColumn<Guid>(
                name: "PublicId",
                table: "FAQs",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWID()");

            migrationBuilder.AddColumn<Guid>(
                name: "PublicId",
                table: "Documents",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWID()");

            migrationBuilder.AddColumn<Guid>(
                name: "PublicId",
                table: "DataRestrictions",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWID()");

            migrationBuilder.AddColumn<Guid>(
                name: "PublicId",
                table: "Customers",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWID()");

            migrationBuilder.AddColumn<Guid>(
                name: "PublicId",
                table: "CustomerRequestStatuses",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWID()");

            migrationBuilder.AddColumn<Guid>(
                name: "PublicId",
                table: "CustomerRequests",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWID()");

            migrationBuilder.AddColumn<Guid>(
                name: "PublicId",
                table: "CustomerRequestMessages",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWID()");

            migrationBuilder.AddColumn<Guid>(
                name: "PublicId",
                table: "CustomerRequestEvaluations",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWID()");

            migrationBuilder.AddColumn<Guid>(
                name: "PublicId",
                table: "CustomerRequestDocuments",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWID()");

            migrationBuilder.AddColumn<Guid>(
                name: "PublicId",
                table: "Conversations",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWID()");

            migrationBuilder.AddColumn<Guid>(
                name: "PublicId",
                table: "ConversationParticipants",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWID()");

            migrationBuilder.AddColumn<Guid>(
                name: "PublicId",
                table: "CompanyModule",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWID()");

            migrationBuilder.AddColumn<Guid>(
                name: "PublicId",
                table: "Companies",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWID()");

            migrationBuilder.AddColumn<Guid>(
                name: "PublicId",
                table: "Accounts",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWID()");

            migrationBuilder.CreateIndex(
                name: "IX_Users_PublicId",
                table: "Users",
                column: "PublicId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_PublicId",
                table: "UserRoles",
                column: "PublicId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserProjects_PublicId",
                table: "UserProjects",
                column: "PublicId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Roles_PublicId",
                table: "Roles",
                column: "PublicId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_PublicId",
                table: "RolePermissions",
                column: "PublicId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissionActions_PublicId",
                table: "RolePermissionActions",
                column: "PublicId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RequestSubjects_PublicId",
                table: "RequestSubjects",
                column: "PublicId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RequestHistories_PublicId",
                table: "RequestHistories",
                column: "PublicId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Projects_PublicId",
                table: "Projects",
                column: "PublicId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectModule_PublicId",
                table: "ProjectModule",
                column: "PublicId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationInfos_PublicId",
                table: "OrganizationInfos",
                column: "PublicId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Modules_PublicId",
                table: "Modules",
                column: "PublicId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ModuleExperts_PublicId",
                table: "ModuleExperts",
                column: "PublicId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MenuItems_PublicId",
                table: "MenuItems",
                column: "PublicId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Guides_PublicId",
                table: "Guides",
                column: "PublicId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FAQs_PublicId",
                table: "FAQs",
                column: "PublicId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Documents_PublicId",
                table: "Documents",
                column: "PublicId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DataRestrictions_PublicId",
                table: "DataRestrictions",
                column: "PublicId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Customers_PublicId",
                table: "Customers",
                column: "PublicId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CustomerRequestStatuses_PublicId",
                table: "CustomerRequestStatuses",
                column: "PublicId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CustomerRequests_PublicId",
                table: "CustomerRequests",
                column: "PublicId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CustomerRequestMessages_PublicId",
                table: "CustomerRequestMessages",
                column: "PublicId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CustomerRequestEvaluations_PublicId",
                table: "CustomerRequestEvaluations",
                column: "PublicId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CustomerRequestDocuments_PublicId",
                table: "CustomerRequestDocuments",
                column: "PublicId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_PublicId",
                table: "Conversations",
                column: "PublicId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ConversationParticipants_PublicId",
                table: "ConversationParticipants",
                column: "PublicId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CompanyModule_PublicId",
                table: "CompanyModule",
                column: "PublicId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Companies_PublicId",
                table: "Companies",
                column: "PublicId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_PublicId",
                table: "Accounts",
                column: "PublicId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_PublicId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_UserRoles_PublicId",
                table: "UserRoles");

            migrationBuilder.DropIndex(
                name: "IX_UserProjects_PublicId",
                table: "UserProjects");

            migrationBuilder.DropIndex(
                name: "IX_Roles_PublicId",
                table: "Roles");

            migrationBuilder.DropIndex(
                name: "IX_RolePermissions_PublicId",
                table: "RolePermissions");

            migrationBuilder.DropIndex(
                name: "IX_RolePermissionActions_PublicId",
                table: "RolePermissionActions");

            migrationBuilder.DropIndex(
                name: "IX_RequestSubjects_PublicId",
                table: "RequestSubjects");

            migrationBuilder.DropIndex(
                name: "IX_RequestHistories_PublicId",
                table: "RequestHistories");

            migrationBuilder.DropIndex(
                name: "IX_Projects_PublicId",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_ProjectModule_PublicId",
                table: "ProjectModule");

            migrationBuilder.DropIndex(
                name: "IX_OrganizationInfos_PublicId",
                table: "OrganizationInfos");

            migrationBuilder.DropIndex(
                name: "IX_Modules_PublicId",
                table: "Modules");

            migrationBuilder.DropIndex(
                name: "IX_ModuleExperts_PublicId",
                table: "ModuleExperts");

            migrationBuilder.DropIndex(
                name: "IX_MenuItems_PublicId",
                table: "MenuItems");

            migrationBuilder.DropIndex(
                name: "IX_Guides_PublicId",
                table: "Guides");

            migrationBuilder.DropIndex(
                name: "IX_FAQs_PublicId",
                table: "FAQs");

            migrationBuilder.DropIndex(
                name: "IX_Documents_PublicId",
                table: "Documents");

            migrationBuilder.DropIndex(
                name: "IX_DataRestrictions_PublicId",
                table: "DataRestrictions");

            migrationBuilder.DropIndex(
                name: "IX_Customers_PublicId",
                table: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_CustomerRequestStatuses_PublicId",
                table: "CustomerRequestStatuses");

            migrationBuilder.DropIndex(
                name: "IX_CustomerRequests_PublicId",
                table: "CustomerRequests");

            migrationBuilder.DropIndex(
                name: "IX_CustomerRequestMessages_PublicId",
                table: "CustomerRequestMessages");

            migrationBuilder.DropIndex(
                name: "IX_CustomerRequestEvaluations_PublicId",
                table: "CustomerRequestEvaluations");

            migrationBuilder.DropIndex(
                name: "IX_CustomerRequestDocuments_PublicId",
                table: "CustomerRequestDocuments");

            migrationBuilder.DropIndex(
                name: "IX_Conversations_PublicId",
                table: "Conversations");

            migrationBuilder.DropIndex(
                name: "IX_ConversationParticipants_PublicId",
                table: "ConversationParticipants");

            migrationBuilder.DropIndex(
                name: "IX_CompanyModule_PublicId",
                table: "CompanyModule");

            migrationBuilder.DropIndex(
                name: "IX_Companies_PublicId",
                table: "Companies");

            migrationBuilder.DropIndex(
                name: "IX_Accounts_PublicId",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "PublicId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PublicId",
                table: "UserRoles");

            migrationBuilder.DropColumn(
                name: "PublicId",
                table: "UserProjects");

            migrationBuilder.DropColumn(
                name: "PublicId",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "PublicId",
                table: "RolePermissions");

            migrationBuilder.DropColumn(
                name: "PublicId",
                table: "RolePermissionActions");

            migrationBuilder.DropColumn(
                name: "PublicId",
                table: "RequestSubjects");

            migrationBuilder.DropColumn(
                name: "PublicId",
                table: "RequestHistories");

            migrationBuilder.DropColumn(
                name: "PublicId",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "PublicId",
                table: "ProjectModule");

            migrationBuilder.DropColumn(
                name: "PublicId",
                table: "OrganizationInfos");

            migrationBuilder.DropColumn(
                name: "PublicId",
                table: "Modules");

            migrationBuilder.DropColumn(
                name: "PublicId",
                table: "ModuleExperts");

            migrationBuilder.DropColumn(
                name: "PublicId",
                table: "MenuItems");

            migrationBuilder.DropColumn(
                name: "PublicId",
                table: "Guides");

            migrationBuilder.DropColumn(
                name: "PublicId",
                table: "FAQs");

            migrationBuilder.DropColumn(
                name: "PublicId",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "PublicId",
                table: "DataRestrictions");

            migrationBuilder.DropColumn(
                name: "PublicId",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "PublicId",
                table: "CustomerRequestStatuses");

            migrationBuilder.DropColumn(
                name: "PublicId",
                table: "CustomerRequests");

            migrationBuilder.DropColumn(
                name: "PublicId",
                table: "CustomerRequestMessages");

            migrationBuilder.DropColumn(
                name: "PublicId",
                table: "CustomerRequestEvaluations");

            migrationBuilder.DropColumn(
                name: "PublicId",
                table: "CustomerRequestDocuments");

            migrationBuilder.DropColumn(
                name: "PublicId",
                table: "Conversations");

            migrationBuilder.DropColumn(
                name: "PublicId",
                table: "ConversationParticipants");

            migrationBuilder.DropColumn(
                name: "PublicId",
                table: "CompanyModule");

            migrationBuilder.DropColumn(
                name: "PublicId",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "PublicId",
                table: "Accounts");
        }
    }
}

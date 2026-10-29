using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace HelpCenter.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveSampleDataSeedFromModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "CompanyModule",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "CompanyModule",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "CompanyModule",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "CompanyModule",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "ConversationParticipants",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "ConversationParticipants",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "CustomerRequestMessages",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "CustomerRequestMessages",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "CustomerRequestMessages",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "CustomerRequestMessages",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "CustomerRequestMessages",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Customers",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "ModuleExperts",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "ModuleExperts",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "ProjectModule",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "ProjectModule",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "ProjectModule",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "ProjectModule",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "ProjectModule",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "RequestHistories",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "RequestHistories",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "RequestHistories",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "RequestHistories",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "RequestHistories",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "UserProjects",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "UserProjects",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "UserProjects",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "UserProjects",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "UserProjects",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Accounts",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "ConversationParticipants",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "ConversationParticipants",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "ConversationParticipants",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "ConversationParticipants",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "ConversationParticipants",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "CustomerRequests",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "CustomerRequests",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "CustomerRequests",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Modules",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Projects",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Accounts",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Conversations",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Conversations",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Conversations",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Customers",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Customers",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Customers",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "RequestSubjects",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "RequestSubjects",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "RequestSubjects",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Accounts",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Accounts",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Accounts",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Accounts",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Accounts",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Modules",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Modules",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Modules",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Projects",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Projects",
                keyColumn: "Id",
                keyValue: 2);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Accounts",
                columns: new[] { "Id", "CreatedAt", "CreatedByAccountId", "Email", "FirstName", "IsActive", "IsDeleted", "IsPasswordChangeRequired", "LastModifiedByAccountId", "LastName", "Password", "PhoneNumber", "ProfilePicture", "UpdatedAt", "Username" },
                values: new object[,]
                {
                    { 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "agent1@helpcenter.com", "Ali", true, false, false, null, "Temsilci", "$2a$11$pCDJIRu1tH/Ir19q7P0fv.1oiPQ4SLP6vOEYU5LjinQtMzn5eL3f2", "+90 532 000 0002", null, null, "agent1.user" },
                    { 3, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "agent2@helpcenter.com", "Berna", true, false, false, null, "KisittiTemsilci", "$2a$11$pCDJIRu1tH/Ir19q7P0fv.1oiPQ4SLP6vOEYU5LjinQtMzn5eL3f2", "+90 532 000 0003", null, null, "agent2.user" },
                    { 4, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "expert@helpcenter.com", "Erol", true, false, false, null, "Uzman", "$2a$11$pCDJIRu1tH/Ir19q7P0fv.1oiPQ4SLP6vOEYU5LjinQtMzn5eL3f2", "+90 532 000 0004", null, null, "expert.user" },
                    { 5, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "ahmet@acme.com", "Ahmet", true, false, false, null, "Yılmaz", "$2a$11$pCDJIRu1tH/Ir19q7P0fv.1oiPQ4SLP6vOEYU5LjinQtMzn5eL3f2", "+90 533 111 0001", null, null, "ahmet.yilmaz" },
                    { 6, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "ayse@acme.com", "Ayşe", true, false, false, null, "Kaya", "$2a$11$pCDJIRu1tH/Ir19q7P0fv.1oiPQ4SLP6vOEYU5LjinQtMzn5eL3f2", "+90 533 111 0002", null, null, "ayse.kaya" },
                    { 7, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "mehmet@beta.com", "Mehmet", true, false, false, null, "Demir", "$2a$11$pCDJIRu1tH/Ir19q7P0fv.1oiPQ4SLP6vOEYU5LjinQtMzn5eL3f2", "+90 533 222 0002", null, null, "mehmet.demir" },
                    { 8, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "can@gamma.com", "Can", true, false, false, null, "Öztürk", "$2a$11$pCDJIRu1tH/Ir19q7P0fv.1oiPQ4SLP6vOEYU5LjinQtMzn5eL3f2", "+90 533 333 0003", null, null, "can.ozturk" }
                });

            migrationBuilder.InsertData(
                table: "Companies",
                columns: new[] { "Id", "Address", "BranchCount", "ContactPersonEmail", "ContactPersonName", "ContactPersonPhone", "ContactPersonSurname", "ContactPersonUsername", "CreatedAt", "CreatedByAccountId", "IsActive", "IsDeleted", "IsDemoActive", "LastModifiedByAccountId", "Mail", "Name", "Phone", "PreviousSystem", "ProjectId", "UpdatedAt" },
                values: new object[] { 3, "Kordon Boyu Cad. No:50 Konak / İzmir", 0, "can@gamma.com", "Can", "+90 533 333 0003", "Öztürk", "can.ozturk", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, false, false, null, "info@gamma.com", "Gamma Danışmanlık", "+90 232 500 0003", "", null, null });

            migrationBuilder.InsertData(
                table: "Conversations",
                columns: new[] { "Id", "CreatedAt", "CreatedByAccountId", "IsActive", "IsDeleted", "LastModifiedByAccountId", "Title", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, false, null, "Acme Fatura Entegrasyon Sorunu", null },
                    { 2, new DateTime(2024, 1, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, false, null, "Beta API Token Hatası", null },
                    { 3, new DateTime(2024, 1, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, false, null, "Acme Stok Güncelleme Talebi", null }
                });

            migrationBuilder.InsertData(
                table: "Modules",
                columns: new[] { "Id", "CreatedAt", "CreatedByAccountId", "Description", "IsActive", "IsDeleted", "IsLocked", "LastModifiedByAccountId", "Name", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "Fatura kesme, e-fatura ve muhasebe fişleri modülü", true, false, false, null, "Muhasebe & Fatura", null },
                    { 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "Sipariş yönetimi ve depo stok hareketleri modülü", true, false, false, null, "Sipariş & Stok", null },
                    { 3, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "Personel bordro hesaplama ve yıllık izin takibi", true, false, false, null, "Bordro & İzin", null },
                    { 4, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "Harici sistemler için REST API ve webhook yönetimi", true, false, false, null, "API & Entegrasyon", null }
                });

            migrationBuilder.InsertData(
                table: "Projects",
                columns: new[] { "Id", "CreatedAt", "CreatedByAccountId", "Description", "IsActive", "IsDeleted", "LastModifiedByAccountId", "Name", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "Kurumsal finans ve ERP entegrasyon projesi", true, false, null, "Finans & ERP Dönüşüm Projesi", null },
                    { 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "B2B ve B2C e-ticaret kanal geliştirme projesi", true, false, null, "E-Ticaret Entegrasyon Projesi", null },
                    { 3, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "İnsan kaynakları ve bordro self-servis portalı", false, false, null, "İK & Bordro Portal Projesi", null }
                });

            migrationBuilder.InsertData(
                table: "Companies",
                columns: new[] { "Id", "Address", "BranchCount", "ContactPersonEmail", "ContactPersonName", "ContactPersonPhone", "ContactPersonSurname", "ContactPersonUsername", "CreatedAt", "CreatedByAccountId", "IsActive", "IsDeleted", "IsDemoActive", "LastModifiedByAccountId", "Mail", "Name", "Phone", "PreviousSystem", "ProjectId", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, "Büyükdere Cad. No:100 Levent / İstanbul", 0, "ahmet@acme.com", "Ahmet", "+90 533 111 0001", "Yılmaz", "ahmet.yilmaz", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, false, false, null, "info@acme.com", "Acme Holding", "+90 212 300 0001", "", 1, null },
                    { 2, "Atatürk Mah. Lojistik Yolu No:20 Tuzla / İstanbul", 0, "mehmet@beta.com", "Mehmet", "+90 533 222 0002", "Demir", "mehmet.demir", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, false, false, null, "info@beta.com", "Beta Lojistik", "+90 216 400 0002", "", 2, null }
                });

            migrationBuilder.InsertData(
                table: "Customers",
                columns: new[] { "Id", "AccountId", "CompanyId", "CreatedAt", "CreatedByAccountId", "IsActive", "IsDeleted", "LastModifiedByAccountId", "UpdatedAt" },
                values: new object[] { 4, 8, 3, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, false, null, null });

            migrationBuilder.InsertData(
                table: "ProjectModule",
                columns: new[] { "Id", "CreatedAt", "CreatedByAccountId", "IsActive", "IsDeleted", "LastModifiedByAccountId", "ModuleId", "ProjectId", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, false, null, 1, 1, null },
                    { 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, false, null, 2, 1, null },
                    { 3, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, false, null, 2, 2, null },
                    { 4, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, false, null, 4, 2, null },
                    { 5, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, false, null, 3, 3, null }
                });

            migrationBuilder.InsertData(
                table: "RequestSubjects",
                columns: new[] { "Id", "CreatedAt", "CreatedByAccountId", "Description", "IsActive", "IsDeleted", "IsLocked", "LastModifiedByAccountId", "ModuleId", "Name", "ProjectId", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "E-Fatura gönderim ve senkronizasyon hataları", true, false, false, null, 1, "Fatura Hataları", null, null },
                    { 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "Pazaryeri ve depo stok miktar uyumsuzlukları", true, false, false, null, 2, "Stok Senkronizasyon Problemi", null, null },
                    { 3, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "Bearer token ve OAuth yetki problemleri", true, false, false, null, 4, "API Kimlik Doğrulama Hatası", null, null }
                });

            migrationBuilder.InsertData(
                table: "UserProjects",
                columns: new[] { "Id", "CreatedAt", "CreatedByAccountId", "IsActive", "IsDeleted", "LastModifiedByAccountId", "ProjectId", "UpdatedAt", "UserId" },
                values: new object[,]
                {
                    { 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, false, null, 1, null, 1 },
                    { 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, false, null, 2, null, 1 }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "AccountId", "CreatedAt", "CreatedByAccountId", "IsActive", "IsDeleted", "LastModifiedByAccountId", "UpdatedAt" },
                values: new object[,]
                {
                    { 2, 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, false, null, null },
                    { 3, 3, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, false, null, null },
                    { 4, 4, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, false, null, null }
                });

            migrationBuilder.InsertData(
                table: "CompanyModule",
                columns: new[] { "Id", "CompanyId", "CreatedAt", "CreatedByAccountId", "IsActive", "IsDeleted", "LastModifiedByAccountId", "ModuleId", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, false, null, 1, null },
                    { 2, 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, false, null, 2, null },
                    { 3, 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, false, null, 2, null },
                    { 4, 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, false, null, 4, null }
                });

            migrationBuilder.InsertData(
                table: "ConversationParticipants",
                columns: new[] { "Id", "ConversationId", "CreatedAt", "CreatedByAccountId", "CustomerId", "IsActive", "IsDeleted", "JoinedAt", "LastModifiedByAccountId", "Type", "UpdatedAt", "UserId" },
                values: new object[,]
                {
                    { 2, 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, true, false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 1, null, 2 },
                    { 3, 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, true, false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 1, null, 4 },
                    { 5, 2, new DateTime(2024, 1, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, true, false, new DateTime(2024, 1, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 1, null, 2 },
                    { 7, 3, new DateTime(2024, 1, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, true, false, new DateTime(2024, 1, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 1, null, 2 }
                });

            migrationBuilder.InsertData(
                table: "Customers",
                columns: new[] { "Id", "AccountId", "CompanyId", "CreatedAt", "CreatedByAccountId", "IsActive", "IsDeleted", "LastModifiedByAccountId", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, 5, 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, false, null, null },
                    { 2, 6, 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, false, null, null },
                    { 3, 7, 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, false, null, null }
                });

            migrationBuilder.InsertData(
                table: "ModuleExperts",
                columns: new[] { "Id", "CreatedAt", "CreatedByAccountId", "IsActive", "IsDeleted", "LastModifiedByAccountId", "ModuleId", "UpdatedAt", "UserId" },
                values: new object[,]
                {
                    { 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, false, null, 1, null, 4 },
                    { 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, false, null, 4, null, 4 }
                });

            migrationBuilder.InsertData(
                table: "UserProjects",
                columns: new[] { "Id", "CreatedAt", "CreatedByAccountId", "IsActive", "IsDeleted", "LastModifiedByAccountId", "ProjectId", "UpdatedAt", "UserId" },
                values: new object[,]
                {
                    { 3, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, false, null, 1, null, 2 },
                    { 4, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, false, null, 2, null, 2 },
                    { 5, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, false, null, 1, null, 3 }
                });

            migrationBuilder.InsertData(
                table: "UserRoles",
                columns: new[] { "Id", "CreatedAt", "CreatedByAccountId", "IsActive", "IsDeleted", "LastModifiedByAccountId", "RoleId", "UpdatedAt", "UserId" },
                values: new object[,]
                {
                    { 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, false, null, 2, null, 2 },
                    { 3, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, false, null, 2, null, 3 },
                    { 4, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, false, null, 4, null, 4 }
                });

            migrationBuilder.InsertData(
                table: "ConversationParticipants",
                columns: new[] { "Id", "ConversationId", "CreatedAt", "CreatedByAccountId", "CustomerId", "IsActive", "IsDeleted", "JoinedAt", "LastModifiedByAccountId", "Type", "UpdatedAt", "UserId" },
                values: new object[,]
                {
                    { 1, 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 1, true, false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 0, null, null },
                    { 4, 2, new DateTime(2024, 1, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 3, true, false, new DateTime(2024, 1, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 0, null, null },
                    { 6, 3, new DateTime(2024, 1, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 2, true, false, new DateTime(2024, 1, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 0, null, null }
                });

            migrationBuilder.InsertData(
                table: "CustomerRequestMessages",
                columns: new[] { "Id", "ConversationId", "CreatedAt", "CreatedByAccountId", "IsActive", "IsDeleted", "IsRead", "LastModifiedByAccountId", "MessageText", "SenderParticipantId", "Type", "UpdatedAt" },
                values: new object[,]
                {
                    { 2, 1, new DateTime(2024, 1, 1, 10, 15, 0, 0, DateTimeKind.Unspecified), null, true, false, true, null, "Talebinizi aldık, teknik uzmanımıza yönlendirdim.", 2, 0, null },
                    { 3, 1, new DateTime(2024, 1, 1, 10, 30, 0, 0, DateTimeKind.Unspecified), null, true, false, false, null, "[İÇ NOT] EF Core veritabanı kilitlenme logları inceleniyor. Müşteri tarafında ek aksiyon gerekmiyor.", 3, 1, null }
                });

            migrationBuilder.InsertData(
                table: "CustomerRequests",
                columns: new[] { "Id", "AssignedUserId", "ConversationId", "CreatedAt", "CreatedByAccountId", "CurrentExpertId", "CustomerId", "Description", "IsActive", "IsDeleted", "LastModifiedByAccountId", "ModuleId", "Priority", "RequestSubjectId", "StatusId", "TicketId", "Title", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, 2, 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 4, 1, "E-Fatura gönderiminde 500 Internal Server Error hatası alınıyor.", true, false, null, 1, 2, 1, 4, "TKT-ACME-001", "Acme Fatura Entegrasyon Sorunu", null },
                    { 2, 2, 2, new DateTime(2024, 1, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, 3, "REST API bearer token süresi beklenenden erken doluyor.", true, false, null, 4, 1, 3, 1, "TKT-BETA-002", "Beta API Token Hatası", null },
                    { 3, 2, 3, new DateTime(2024, 1, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, 2, "Depo stokları güncellendi, kontroller tamamlandı.", true, false, null, 2, 0, 2, 3, "TKT-ACME-003", "Acme Stok Güncelleme Talebi", null }
                });

            migrationBuilder.InsertData(
                table: "CustomerRequestMessages",
                columns: new[] { "Id", "ConversationId", "CreatedAt", "CreatedByAccountId", "IsActive", "IsDeleted", "IsRead", "LastModifiedByAccountId", "MessageText", "SenderParticipantId", "Type", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, 1, new DateTime(2024, 1, 1, 10, 0, 0, 0, DateTimeKind.Unspecified), null, true, false, true, null, "Fatura keserken 500 hatası alıyoruz.", 1, 0, null },
                    { 4, 2, new DateTime(2024, 1, 2, 11, 0, 0, 0, DateTimeKind.Unspecified), null, true, false, false, null, "REST API token 1 saat sonra geçersiz oluyor.", 4, 0, null },
                    { 5, 3, new DateTime(2024, 1, 3, 14, 0, 0, 0, DateTimeKind.Unspecified), null, true, false, true, null, "Stok listesi başarıyla güncellendi, teşekkürler.", 6, 0, null }
                });

            migrationBuilder.InsertData(
                table: "RequestHistories",
                columns: new[] { "Id", "Action", "ActorAccountId", "CreatedAt", "CreatedByAccountId", "IsActive", "IsDeleted", "LastModifiedByAccountId", "NewValue", "Note", "OldValue", "RequestId", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, 1, 5, new DateTime(2024, 1, 1, 10, 0, 0, 0, DateTimeKind.Unspecified), null, true, false, null, null, "Talep Oluşturuldu", null, 1, null },
                    { 2, 3, 2, new DateTime(2024, 1, 1, 10, 20, 0, 0, DateTimeKind.Unspecified), null, true, false, null, null, "Uzmana Yönlendirildi", null, 1, null },
                    { 3, 1, 7, new DateTime(2024, 1, 2, 11, 0, 0, 0, DateTimeKind.Unspecified), null, true, false, null, null, "Talep Oluşturuldu", null, 2, null },
                    { 4, 1, 6, new DateTime(2024, 1, 3, 14, 0, 0, 0, DateTimeKind.Unspecified), null, true, false, null, null, "Talep Oluşturuldu", null, 3, null },
                    { 5, 2, 2, new DateTime(2024, 1, 3, 16, 0, 0, 0, DateTimeKind.Unspecified), null, true, false, null, null, "Talep Kapatıldı", null, 3, null }
                });
        }
    }
}

using HelpCenter.Domain.Entities;

namespace HelpCenter.Persistence.Context.Seed.SampleData;

/// <summary>
/// Sample Project/Module data. No longer embedded into migrations via ModelBuilder.HasData() —
/// this class only produces plain object lists; actual insertion is done by SampleDataSeeder
/// at runtime (only in Development/Staging). See SampleDataSeeder.
/// </summary>
public static class SampleProjectModuleSeed
{
    public static IReadOnlyList<Project> BuildProjects() =>
    [
        new Project { PublicId = SeedIdentity.FromLegacyInt(1), Id = SeedIdentity.FromLegacyInt(1), Name = "Finans & ERP Dönüşüm Projesi", Description = "Kurumsal finans ve ERP entegrasyon projesi", IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },
        new Project { PublicId = SeedIdentity.FromLegacyInt(2), Id = SeedIdentity.FromLegacyInt(2), Name = "E-Ticaret Entegrasyon Projesi", Description = "B2B ve B2C e-ticaret kanal geliştirme projesi", IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },
        new Project { PublicId = SeedIdentity.FromLegacyInt(3), Id = SeedIdentity.FromLegacyInt(3), Name = "İK & Bordro Portal Projesi", Description = "İnsan kaynakları ve bordro self-servis portalı", IsActive = false, CreatedAt = new DateTime(2024, 1, 1) }
    ];

    public static IReadOnlyList<Module> BuildModules() =>
    [
        new Module { PublicId = SeedIdentity.FromLegacyInt(1), Id = SeedIdentity.FromLegacyInt(1), Name = "Muhasebe & Fatura", Description = "Fatura kesme, e-fatura ve muhasebe fişleri modülü", IsLocked = false, IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },
        new Module { PublicId = SeedIdentity.FromLegacyInt(2), Id = SeedIdentity.FromLegacyInt(2), Name = "Sipariş & Stok", Description = "Sipariş yönetimi ve depo stok hareketleri modülü", IsLocked = false, IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },
        new Module { PublicId = SeedIdentity.FromLegacyInt(3), Id = SeedIdentity.FromLegacyInt(3), Name = "Bordro & İzin", Description = "Personel bordro hesaplama ve yıllık izin takibi", IsLocked = false, IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },
        new Module { PublicId = SeedIdentity.FromLegacyInt(4), Id = SeedIdentity.FromLegacyInt(4), Name = "API & Entegrasyon", Description = "Harici sistemler için REST API ve webhook yönetimi", IsLocked = false, IsActive = true, CreatedAt = new DateTime(2024, 1, 1) }
    ];

    public static IReadOnlyList<ProjectModule> BuildProjectModules() =>
    [
        new ProjectModule { PublicId = SeedIdentity.FromLegacyInt(1), Id = SeedIdentity.FromLegacyInt(1), ProjectId = SeedIdentity.FromLegacyInt(1), ModuleId = SeedIdentity.FromLegacyInt(1), IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },
        new ProjectModule { PublicId = SeedIdentity.FromLegacyInt(2), Id = SeedIdentity.FromLegacyInt(2), ProjectId = SeedIdentity.FromLegacyInt(1), ModuleId = SeedIdentity.FromLegacyInt(2), IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },
        new ProjectModule { PublicId = SeedIdentity.FromLegacyInt(3), Id = SeedIdentity.FromLegacyInt(3), ProjectId = SeedIdentity.FromLegacyInt(2), ModuleId = SeedIdentity.FromLegacyInt(2), IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },
        new ProjectModule { PublicId = SeedIdentity.FromLegacyInt(4), Id = SeedIdentity.FromLegacyInt(4), ProjectId = SeedIdentity.FromLegacyInt(2), ModuleId = SeedIdentity.FromLegacyInt(4), IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },
        new ProjectModule { PublicId = SeedIdentity.FromLegacyInt(5), Id = SeedIdentity.FromLegacyInt(5), ProjectId = SeedIdentity.FromLegacyInt(3), ModuleId = SeedIdentity.FromLegacyInt(3), IsActive = true, CreatedAt = new DateTime(2024, 1, 1) }
    ];

    public static IReadOnlyList<RequestSubject> BuildRequestSubjects() =>
    [
        new RequestSubject { PublicId = SeedIdentity.FromLegacyInt(1), Id = SeedIdentity.FromLegacyInt(1), Name = "Fatura Hataları", Description = "E-Fatura gönderim ve senkronizasyon hataları", ModuleId = SeedIdentity.FromLegacyInt(1), IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },
        new RequestSubject { PublicId = SeedIdentity.FromLegacyInt(2), Id = SeedIdentity.FromLegacyInt(2), Name = "Stok Senkronizasyon Problemi", Description = "Pazaryeri ve depo stok miktar uyumsuzlukları", ModuleId = SeedIdentity.FromLegacyInt(2), IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },
        new RequestSubject { PublicId = SeedIdentity.FromLegacyInt(3), Id = SeedIdentity.FromLegacyInt(3), Name = "API Kimlik Doğrulama Hatası", Description = "Bearer token ve OAuth yetki problemleri", ModuleId = SeedIdentity.FromLegacyInt(4), IsActive = true, CreatedAt = new DateTime(2024, 1, 1) }
    ];
}

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
        new Project { Id = 1, Name = "Finans & ERP Dönüşüm Projesi", Description = "Kurumsal finans ve ERP entegrasyon projesi", IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },
        new Project { Id = 2, Name = "E-Ticaret Entegrasyon Projesi", Description = "B2B ve B2C e-ticaret kanal geliştirme projesi", IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },
        new Project { Id = 3, Name = "İK & Bordro Portal Projesi", Description = "İnsan kaynakları ve bordro self-servis portalı", IsActive = false, CreatedAt = new DateTime(2024, 1, 1) }
    ];

    public static IReadOnlyList<Module> BuildModules() =>
    [
        new Module { Id = 1, Name = "Muhasebe & Fatura", Description = "Fatura kesme, e-fatura ve muhasebe fişleri modülü", IsLocked = false, IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },
        new Module { Id = 2, Name = "Sipariş & Stok", Description = "Sipariş yönetimi ve depo stok hareketleri modülü", IsLocked = false, IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },
        new Module { Id = 3, Name = "Bordro & İzin", Description = "Personel bordro hesaplama ve yıllık izin takibi", IsLocked = false, IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },
        new Module { Id = 4, Name = "API & Entegrasyon", Description = "Harici sistemler için REST API ve webhook yönetimi", IsLocked = false, IsActive = true, CreatedAt = new DateTime(2024, 1, 1) }
    ];

    public static IReadOnlyList<ProjectModule> BuildProjectModules() =>
    [
        new ProjectModule { Id = 1, ProjectId = 1, ModuleId = 1, IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },
        new ProjectModule { Id = 2, ProjectId = 1, ModuleId = 2, IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },
        new ProjectModule { Id = 3, ProjectId = 2, ModuleId = 2, IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },
        new ProjectModule { Id = 4, ProjectId = 2, ModuleId = 4, IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },
        new ProjectModule { Id = 5, ProjectId = 3, ModuleId = 3, IsActive = true, CreatedAt = new DateTime(2024, 1, 1) }
    ];

    public static IReadOnlyList<RequestSubject> BuildRequestSubjects() =>
    [
        new RequestSubject { Id = 1, Name = "Fatura Hataları", Description = "E-Fatura gönderim ve senkronizasyon hataları", ModuleId = 1, IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },
        new RequestSubject { Id = 2, Name = "Stok Senkronizasyon Problemi", Description = "Pazaryeri ve depo stok miktar uyumsuzlukları", ModuleId = 2, IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },
        new RequestSubject { Id = 3, Name = "API Kimlik Doğrulama Hatası", Description = "Bearer token ve OAuth yetki problemleri", ModuleId = 4, IsActive = true, CreatedAt = new DateTime(2024, 1, 1) }
    ];
}

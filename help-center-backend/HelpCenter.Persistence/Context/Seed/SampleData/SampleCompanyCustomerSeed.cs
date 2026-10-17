using HelpCenter.Domain.Entities;

namespace HelpCenter.Persistence.Context.Seed.SampleData;

/// <summary>
/// Sample company/customer data ("Acme Holding", etc.). No longer embedded into
/// migrations via ModelBuilder.HasData() — this class only produces plain object lists;
/// actual insertion is done by SampleDataSeeder at runtime (only in Development/Staging).
/// </summary>
public static class SampleCompanyCustomerSeed
{
    private const string DefaultPasswordHash = "$2a$11$pCDJIRu1tH/Ir19q7P0fv.1oiPQ4SLP6vOEYU5LjinQtMzn5eL3f2";

    public static IReadOnlyList<Company> BuildCompanies() =>
    [
        new Company
        {
            PublicId = SeedIdentity.FromLegacyInt(1), Id = SeedIdentity.FromLegacyInt(1),
            Name = "Acme Holding",
            ProjectId = SeedIdentity.FromLegacyInt(1),
            Address = "Büyükdere Cad. No:100 Levent / İstanbul",
            Phone = "+90 212 300 0001",
            Mail = "info@acme.com",
            ContactPersonName = "Ahmet",
            ContactPersonSurname = "Yılmaz",
            ContactPersonEmail = "ahmet@acme.com",
            ContactPersonPhone = "+90 533 111 0001",
            ContactPersonUsername = "ahmet.yilmaz",
            IsActive = true,
            CreatedAt = new DateTime(2024, 1, 1)
        },
        new Company
        {
            PublicId = SeedIdentity.FromLegacyInt(2), Id = SeedIdentity.FromLegacyInt(2),
            Name = "Beta Lojistik",
            ProjectId = SeedIdentity.FromLegacyInt(2),
            Address = "Atatürk Mah. Lojistik Yolu No:20 Tuzla / İstanbul",
            Phone = "+90 216 400 0002",
            Mail = "info@beta.com",
            ContactPersonName = "Mehmet",
            ContactPersonSurname = "Demir",
            ContactPersonEmail = "mehmet@beta.com",
            ContactPersonPhone = "+90 533 222 0002",
            ContactPersonUsername = "mehmet.demir",
            IsActive = true,
            CreatedAt = new DateTime(2024, 1, 1)
        },
        new Company
        {
            PublicId = SeedIdentity.FromLegacyInt(3), Id = SeedIdentity.FromLegacyInt(3),
            Name = "Gamma Danışmanlık",
            ProjectId = null,
            Address = "Kordon Boyu Cad. No:50 Konak / İzmir",
            Phone = "+90 232 500 0003",
            Mail = "info@gamma.com",
            ContactPersonName = "Can",
            ContactPersonSurname = "Öztürk",
            ContactPersonEmail = "can@gamma.com",
            ContactPersonPhone = "+90 533 333 0003",
            ContactPersonUsername = "can.ozturk",
            IsActive = true,
            CreatedAt = new DateTime(2024, 1, 1)
        }
    ];

    // Licensed Department Modules
    public static IReadOnlyList<CompanyModule> BuildCompanyModules() =>
    [
        new CompanyModule { PublicId = SeedIdentity.FromLegacyInt(1), Id = SeedIdentity.FromLegacyInt(1), CompanyId = SeedIdentity.FromLegacyInt(1), ModuleId = SeedIdentity.FromLegacyInt(1), IsActive = true, CreatedAt = new DateTime(2024, 1, 1) }, // Acme -> Muhasebe
        new CompanyModule { PublicId = SeedIdentity.FromLegacyInt(2), Id = SeedIdentity.FromLegacyInt(2), CompanyId = SeedIdentity.FromLegacyInt(1), ModuleId = SeedIdentity.FromLegacyInt(2), IsActive = true, CreatedAt = new DateTime(2024, 1, 1) }, // Acme -> Stok
        new CompanyModule { PublicId = SeedIdentity.FromLegacyInt(3), Id = SeedIdentity.FromLegacyInt(3), CompanyId = SeedIdentity.FromLegacyInt(2), ModuleId = SeedIdentity.FromLegacyInt(2), IsActive = true, CreatedAt = new DateTime(2024, 1, 1) }, // Beta -> Stok
        new CompanyModule { PublicId = SeedIdentity.FromLegacyInt(4), Id = SeedIdentity.FromLegacyInt(4), CompanyId = SeedIdentity.FromLegacyInt(2), ModuleId = SeedIdentity.FromLegacyInt(4), IsActive = true, CreatedAt = new DateTime(2024, 1, 1) }  // Beta -> API
    ];

    // Customer Accounts (Id: 5..8)
    public static IReadOnlyList<Account> BuildCustomerAccounts() =>
    [
        // Acme Customer 1
        new Account
        {
            PublicId = SeedIdentity.FromLegacyInt(5), Id = SeedIdentity.FromLegacyInt(5),
            Email = "ahmet@acme.com",
            Username = "ahmet.yilmaz",
            FirstName = "Ahmet",
            LastName = "Yılmaz",
            Password = DefaultPasswordHash,
            PhoneNumber = "+90 533 111 0001",
            IsActive = true,
            CreatedAt = new DateTime(2024, 1, 1)
        },
        // Acme Customer 2
        new Account
        {
            PublicId = SeedIdentity.FromLegacyInt(6), Id = SeedIdentity.FromLegacyInt(6),
            Email = "ayse@acme.com",
            Username = "ayse.kaya",
            FirstName = "Ayşe",
            LastName = "Kaya",
            Password = DefaultPasswordHash,
            PhoneNumber = "+90 533 111 0002",
            IsActive = true,
            CreatedAt = new DateTime(2024, 1, 1)
        },
        // Beta Customer 3
        new Account
        {
            PublicId = SeedIdentity.FromLegacyInt(7), Id = SeedIdentity.FromLegacyInt(7),
            Email = "mehmet@beta.com",
            Username = "mehmet.demir",
            FirstName = "Mehmet",
            LastName = "Demir",
            Password = DefaultPasswordHash,
            PhoneNumber = "+90 533 222 0002",
            IsActive = true,
            CreatedAt = new DateTime(2024, 1, 1)
        },
        // Gamma Customer 4
        new Account
        {
            PublicId = SeedIdentity.FromLegacyInt(8), Id = SeedIdentity.FromLegacyInt(8),
            Email = "can@gamma.com",
            Username = "can.ozturk",
            FirstName = "Can",
            LastName = "Öztürk",
            Password = DefaultPasswordHash,
            PhoneNumber = "+90 533 333 0003",
            IsActive = true,
            CreatedAt = new DateTime(2024, 1, 1)
        }
    ];

    public static IReadOnlyList<Customer> BuildCustomers() =>
    [
        new Customer { PublicId = SeedIdentity.FromLegacyInt(1), Id = SeedIdentity.FromLegacyInt(1), AccountId = SeedIdentity.FromLegacyInt(5), CompanyId = SeedIdentity.FromLegacyInt(1), IsActive = true, CreatedAt = new DateTime(2024, 1, 1) }, // Ahmet -> Acme
        new Customer { PublicId = SeedIdentity.FromLegacyInt(2), Id = SeedIdentity.FromLegacyInt(2), AccountId = SeedIdentity.FromLegacyInt(6), CompanyId = SeedIdentity.FromLegacyInt(1), IsActive = true, CreatedAt = new DateTime(2024, 1, 1) }, // Ayse -> Acme
        new Customer { PublicId = SeedIdentity.FromLegacyInt(3), Id = SeedIdentity.FromLegacyInt(3), AccountId = SeedIdentity.FromLegacyInt(7), CompanyId = SeedIdentity.FromLegacyInt(2), IsActive = true, CreatedAt = new DateTime(2024, 1, 1) }, // Mehmet -> Beta
        new Customer { PublicId = SeedIdentity.FromLegacyInt(4), Id = SeedIdentity.FromLegacyInt(4), AccountId = SeedIdentity.FromLegacyInt(8), CompanyId = SeedIdentity.FromLegacyInt(3), IsActive = true, CreatedAt = new DateTime(2024, 1, 1) }  // Can -> Gamma
    ];
}

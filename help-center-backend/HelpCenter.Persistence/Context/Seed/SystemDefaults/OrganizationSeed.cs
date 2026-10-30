using HelpCenter.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HelpCenter.Persistence.Context.Seed.SystemDefaults;

public static class OrganizationSeed
{
    public static void SeedOrganizations(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OrganizationInfo>().HasData(
            new OrganizationInfo
            {
                PublicId = SeedIdentity.FromLegacyInt(1), Id = SeedIdentity.FromLegacyInt(1),
                OrganizationName = "Help Center",
                Phone = "+90 (212) 555 0100",
                Email = "info@helpcenter.com",
                Website = "https://helpcenter.com",
                TaxNumber = "1234567890",
                TaxOffice = "Maslak V.D.",
                IsActive = true,
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        );
    }
}

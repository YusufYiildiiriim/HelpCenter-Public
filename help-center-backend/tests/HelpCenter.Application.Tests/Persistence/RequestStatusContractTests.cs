using HelpCenter.Domain.Constants;
using HelpCenter.Domain.Entities;
using HelpCenter.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Xunit;

namespace HelpCenter.Application.Tests.Persistence;

public class RequestStatusContractTests
{
    [Fact]
    public void System_status_seeds_keep_the_stable_wire_contract_without_migration()
    {
        using var db = new EfContext(new DbContextOptionsBuilder<EfContext>()
            .UseSqlServer("Server=localhost;Database=ModelOnly;Integrated Security=true;TrustServerCertificate=true").Options);
        var seeds = db.GetService<IDesignTimeModel>().Model.FindEntityType(typeof(CustomerRequestStatus))!.GetSeedData().ToList();
        var expected = new Dictionary<int, string>
        {
            [RequestStatusConstants.WaitingForReply] = "Cevap Bekliyor",
            [RequestStatusConstants.Replied] = "Cevaplandı",
            [RequestStatusConstants.Completed] = "Tamamlandı",
            [RequestStatusConstants.UnderTechnicalReview] = "Teknik İncelemede"
        };
        Assert.Equal(new[] { 1, 2, 3, 4 }, expected.Keys);
        Assert.Equal(4, seeds.Count);
        foreach (var seed in seeds) Assert.Equal(expected[(int)seed["Id"]!], seed["Name"]);
        Assert.False(db.Database.HasPendingModelChanges());
    }
}
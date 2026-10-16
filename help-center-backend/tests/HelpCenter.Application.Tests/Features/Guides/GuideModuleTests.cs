using HelpCenter.Application.Exceptions;
using HelpCenter.Application.Features.Guides;
using HelpCenter.Application.Tests.Common;
using HelpCenter.Domain.Entities;
using Xunit;

namespace HelpCenter.Application.Tests.Features.Guides;

public class GuideModuleTests : HandlerTestBase
{
    [Fact]
    public async Task Legacy_name_requires_one_match_and_explicit_id_resolves_duplicate_names()
    {
        await Assert.ThrowsAsync<ModuleNotFoundException>(() => GuideModuleResolver.ResolveAsync(Uow, null, "Missing", default));
        var first = new Module { Name = "Shared" };
        Db.Modules.Add(first);
        await Db.SaveChangesAsync();
        Assert.Equal(first.Id, (await GuideModuleResolver.ResolveAsync(Uow, null, "Shared", default)).Id);
        Db.Modules.Add(new Module { Name = "Shared" });
        await Db.SaveChangesAsync();
        await Assert.ThrowsAsync<ModuleNotFoundException>(() => GuideModuleResolver.ResolveAsync(Uow, null, "Shared", default));
        Assert.Equal(first.Id, (await GuideModuleResolver.ResolveAsync(Uow, first.Id, "Old display name", default)).Id);
    }
}

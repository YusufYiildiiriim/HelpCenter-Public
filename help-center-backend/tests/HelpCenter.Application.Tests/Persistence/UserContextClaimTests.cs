using System.Security.Claims;
using HelpCenter.Application.Common.Security;
using HelpCenter.Application.Interfaces;
using HelpCenter.Persistence.Services;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace HelpCenter.Application.Tests.Persistence;

public sealed class UserContextClaimTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Http_and_hub_contexts_read_existing_claim_names_consistently(bool hub)
    {
        // Literal names intentionally lock the external token contract across refactors.
        var principal = Principal(new Claim("UserId", TestIds.FromInt(42).ToString()), new Claim("CompanyId", TestIds.FromInt(7).ToString()),
            new Claim("RoleId", TestIds.FromInt(3).ToString()), new Claim("RoleId", "invalid"), new Claim("RoleId", Guid.Empty.ToString()),
            new Claim(ClaimTypes.Role, "Customer"));
        var context = Context(principal, hub);

        Assert.True(context.IsAuthenticated);
        Assert.Equal(TestIds.FromInt(42), context.UserId);
        Assert.Equal(TestIds.FromInt(7), context.CompanyId);
        Assert.Equal(new[] { TestIds.FromInt(3) }, context.RoleIds);
        Assert.True(context.IsCustomer);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("not-an-id")]
    [InlineData("0")]
    [InlineData("-1")]
    public void Missing_or_invalid_company_claim_has_no_company_scope(string? company)
    {
        var principal = company == null ? Principal() : Principal(new Claim(AppClaimTypes.CompanyId, company));
        Assert.Null(Context(principal, hub: false).CompanyId);
        Assert.Null(Context(principal, hub: true).CompanyId);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Staff_identity_falls_back_to_name_identifier_without_inventing_company_scope(bool hub)
    {
        var context = Context(Principal(new Claim(ClaimTypes.NameIdentifier, TestIds.FromInt(17).ToString()), new Claim(ClaimTypes.Role, "Admin")), hub);

        Assert.Equal(TestIds.FromInt(17), context.UserId);
        Assert.Null(context.CompanyId);
        Assert.False(context.IsCustomer);
    }

    [Fact]
    public void Missing_principal_is_anonymous_in_both_contexts()
    {
        foreach (var context in new IUserContext[] { new HttpBasedUserContext(new HttpContextAccessor()), new HubUserContext(null) })
        {
            Assert.False(context.IsAuthenticated);
            Assert.Equal(Guid.Empty, context.UserId);
            Assert.Null(context.CompanyId);
            Assert.Empty(context.RoleIds);
        }
    }

    private static ClaimsPrincipal Principal(params Claim[] claims) => new(new ClaimsIdentity(claims, "test"));
    private static IUserContext Context(ClaimsPrincipal principal, bool hub) => hub ? new HubUserContext(principal)
        : new HttpBasedUserContext(new HttpContextAccessor { HttpContext = new DefaultHttpContext { User = principal } });
}

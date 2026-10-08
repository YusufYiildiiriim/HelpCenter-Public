using HelpCenter.WebApi.ServiceRegistration;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace HelpCenter.Application.Tests.WebApi;

public class CorsRetryAfterTests
{
    [Fact]
    public async Task Allowed_frontend_can_read_retry_after_on_a_rate_limited_response()
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["AppSettings:FrontendUrl"] = "https://helpcenter.example",
            ["JwtSettings:SecretKey"] = new string('x', 64)
        }).Build();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddWebApiServices(configuration);
        using var provider = services.BuildServiceProvider();
        var policyProvider = provider.GetRequiredService<ICorsPolicyProvider>();
        var context = new DefaultHttpContext();
        context.Request.Headers.Origin = "http://localhost:3000";
        context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        context.Response.Headers.RetryAfter = "12";
        var policy = await policyProvider.GetPolicyAsync(context, "AllowNextJs");
        var cors = provider.GetRequiredService<ICorsService>();
        cors.ApplyResult(cors.EvaluatePolicy(context, policy!), context.Response);

        Assert.Equal("http://localhost:3000", context.Response.Headers.AccessControlAllowOrigin.ToString());
        Assert.Contains("Retry-After", context.Response.Headers.AccessControlExposeHeaders.ToString());
        Assert.Equal("12", context.Response.Headers.RetryAfter.ToString());
    }
}

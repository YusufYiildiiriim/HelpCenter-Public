using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using HelpCenter.WebApi.IntegrationTests.Infrastructure;
using Xunit;

namespace HelpCenter.WebApi.IntegrationTests;

[Collection(IntegrationTestCollection.Name)]
public sealed class AuthSessionEndpointsTests(SqlServerWebApplicationFactory factory)
{
    [Fact]
    public async Task AdminLogin_sets_secure_refresh_cookie_without_leaking_it_in_json()
    {
        var response = await LoginAdminAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("\"refreshToken\"", body, StringComparison.OrdinalIgnoreCase);
        Assert.False(string.IsNullOrWhiteSpace(ReadAccessToken(body)));
        AssertSecureRefreshCookie(response);
    }

    [Fact]
    public async Task CustomerLogin_sets_secure_refresh_cookie_without_leaking_it_in_json()
    {
        await IntegrationAuthTestData.EnsureSeededAsync(factory);
        using var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/customer-login", new
        {
            email = IntegrationAuthTestData.CustomerEmail,
            password = IntegrationAuthTestData.Password
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("\"refreshToken\"", body, StringComparison.OrdinalIgnoreCase);
        Assert.False(string.IsNullOrWhiteSpace(ReadAccessToken(body)));
        AssertSecureRefreshCookie(response);
    }

    [Fact]
    public async Task Protected_endpoint_rejects_anonymous_requests()
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/auth/verify");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Refresh_rotates_cookie_and_does_not_return_refresh_secret_in_json()
    {
        using var login = await LoginAdminAsync();
        var initialCookie = ReadRefreshCookie(login);

        using var client = factory.CreateClient();
        using var request = CookieRequest(HttpMethod.Post, "/api/auth/refresh", initialCookie);
        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("\"refreshToken\"", body, StringComparison.OrdinalIgnoreCase);
        Assert.False(string.IsNullOrWhiteSpace(ReadAccessToken(body)));
        AssertSecureRefreshCookie(response);
        Assert.NotEqual(initialCookie, ReadRefreshCookie(response));
    }

    [Fact]
    public async Task Logout_revokes_refresh_cookie_so_it_cannot_be_used_again()
    {
        using var login = await LoginAdminAsync();
        var refreshCookie = ReadRefreshCookie(login);
        using var client = factory.CreateClient();

        using var logout = CookieRequest(HttpMethod.Post, "/api/auth/logout", refreshCookie);
        var logoutResponse = await client.SendAsync(logout);
        Assert.Equal(HttpStatusCode.OK, logoutResponse.StatusCode);

        using var refresh = CookieRequest(HttpMethod.Post, "/api/auth/refresh", refreshCookie);
        var refreshResponse = await client.SendAsync(refresh);
        Assert.Equal(HttpStatusCode.Unauthorized, refreshResponse.StatusCode);
    }

    internal async Task<HttpResponseMessage> LoginAdminAsync()
    {
        await IntegrationAuthTestData.EnsureSeededAsync(factory);
        var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/admin-login", new
        {
            email = IntegrationAuthTestData.AdminEmail,
            password = IntegrationAuthTestData.Password
        });
        client.Dispose();
        return response;
    }

    internal static HttpRequestMessage CookieRequest(HttpMethod method, string path, string cookie)
    {
        var request = new HttpRequestMessage(method, path);
        request.Headers.Add("Cookie", $"refresh_token={cookie}");
        return request;
    }

    internal static string ReadRefreshCookie(HttpResponseMessage response)
    {
        var cookie = response.Headers.GetValues("Set-Cookie")
            .Single(value => value.StartsWith("refresh_token=", StringComparison.OrdinalIgnoreCase));
        return cookie.Split(';', 2)[0]["refresh_token=".Length..];
    }

    internal static void AssertSecureRefreshCookie(HttpResponseMessage response)
    {
        var cookie = response.Headers.GetValues("Set-Cookie")
            .Single(value => value.StartsWith("refresh_token=", StringComparison.OrdinalIgnoreCase));
        Assert.Contains("HttpOnly", cookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Secure", cookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("SameSite=Lax", cookie, StringComparison.OrdinalIgnoreCase);
    }

    internal static string? ReadAccessToken(string body)
    {
        using var document = JsonDocument.Parse(body);
        var root = document.RootElement;
        var tokenContainer = root.TryGetProperty("data", out var data) ? data : root;
        return tokenContainer.TryGetProperty("accessToken", out var accessToken)
            ? accessToken.GetString()
            : tokenContainer.TryGetProperty("token", out var token) ? token.GetString() : null;
    }
}

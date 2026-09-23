using HelpCenter.WebApi.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR.Client;
using Xunit;

namespace HelpCenter.WebApi.IntegrationTests;

[Collection(IntegrationTestCollection.Name)]
public sealed class RequestHubAuthenticationTests(SqlServerWebApplicationFactory factory)
{
    [Fact]
    public async Task Anonymous_client_cannot_connect_to_request_hub()
    {
        await using var connection = new HubConnectionBuilder()
            .WithUrl("http://localhost/requestHub", options =>
            {
                options.HttpMessageHandlerFactory = _ => factory.Server.CreateHandler();
                options.Transports = HttpTransportType.LongPolling;
            })
            .Build();

        await Assert.ThrowsAnyAsync<Exception>(() => connection.StartAsync());
    }

    [Fact]
    public async Task Authenticated_admin_can_connect_to_request_hub()
    {
        var sessions = new AuthSessionEndpointsTests(factory);
        using var login = await sessions.LoginAdminAsync();
        var accessToken = AuthSessionEndpointsTests.ReadAccessToken(await login.Content.ReadAsStringAsync());
        Assert.False(string.IsNullOrWhiteSpace(accessToken));

        await using var connection = new HubConnectionBuilder()
            .WithUrl("http://localhost/requestHub", options =>
            {
                options.AccessTokenProvider = () => Task.FromResult<string?>(accessToken);
                options.HttpMessageHandlerFactory = _ => factory.Server.CreateHandler();
                options.Transports = HttpTransportType.LongPolling;
            })
            .Build();

        await connection.StartAsync();

        Assert.Equal(HubConnectionState.Connected, connection.State);
    }
}

using FluentAssertions;
using HelpCenter.Domain.Constants;
using HelpCenter.WebApi.Hubs;
using Microsoft.AspNetCore.SignalR;
using NSubstitute;
using Xunit;

namespace HelpCenter.Application.Tests.Features.Requests;

public class RequestStatusHubTests
{
    [Fact]
    public async Task Status_notification_targets_only_the_authorized_request_group_with_camel_case_ready_payload()
    {
        var context = Substitute.For<IHubContext<RequestHub>>();
        var proxy = Substitute.For<IClientProxy>();
        var requestId = Guid.NewGuid().ToString();
        context.Clients.Group(requestId).Returns(proxy);
        await new RequestHubService(context).NotifyRequestStatusChanged(requestId,
            RequestStatusConstants.Completed, "Tamamlandı", default);
        await proxy.Received(1).SendCoreAsync("RequestStatusChanged", Arg.Is<object?[]>(args =>
            args.Length == 1 && args[0]!.GetType().GetProperty("RequestPublicId")!.GetValue(args[0])!.Equals(requestId)
            && args[0]!.GetType().GetProperty("StatusId")!.GetValue(args[0])!.Equals(RequestStatusConstants.Completed)), default);
        context.Clients.ReceivedCalls().Should().NotContain(call => call.GetMethodInfo().Name == "get_All");
    }
}

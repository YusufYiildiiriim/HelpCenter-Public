using FluentAssertions;
using HelpCenter.Application.Features.Requests.Commands.CloseRequest;
using HelpCenter.Application.Interfaces;
using HelpCenter.Application.Tests.Common;
using HelpCenter.Domain.Constants;
using HelpCenter.Domain.Entities;
using HelpCenter.Domain.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Xunit;

namespace HelpCenter.Application.Tests.Features.Requests;

public class CloseRequestNotificationTests : HandlerTestBase
{
    private readonly IRequestHubService _hub = Substitute.For<IRequestHubService>();
    private readonly ILogger<CloseRequestCommandHandler> _logger = Substitute.For<ILogger<CloseRequestCommandHandler>>();

    private CustomerRequest Seed()
    {
        var customer = new Customer { Company = new Company { Name = "Test Co" },
            Account = new Account { Email = "close@example.test", Username = "close", Password = "hash", FirstName = "Test", LastName = "Customer" } };
        Db.Customers.Add(customer);
        Db.SaveChanges();
        var request = CustomerRequest.Create(customer.Id, null, null, "Close test", RequestPriority.Low);
        Db.CustomerRequests.Add(request);
        Db.SaveChanges();
        Db.ChangeTracker.Clear();
        return request;
    }

    [Fact]
    public async Task Close_notifies_only_after_persistence_and_repeat_does_not_duplicate_evaluation()
    {
        var request = Seed();
        _hub.NotifyRequestStatusChanged(request.PublicId.ToString(), RequestStatusConstants.Completed, "Tamamlandı", default)
            .Returns(_ => {
                Db.CustomerRequests.AsNoTracking().Single(r => r.Id == request.Id).StatusId.Should().Be(RequestStatusConstants.Completed);
                Db.CustomerRequestEvaluations.Count().Should().Be(1);
                return Task.CompletedTask;
            });
        var handler = new CloseRequestCommandHandler(Uow, _hub, _logger);
        var command = new CloseRequestCommand { PublicId = request.PublicId, CustomerId = request.CustomerId, Rating = 4 };
        await handler.Handle(command, default);
        var historyCount = Db.RequestHistories.Count();
        Db.ChangeTracker.Clear();
        await handler.Handle(command, default);
        Db.ChangeTracker.Clear();
        Db.CustomerRequestEvaluations.Count().Should().Be(1);
        Db.RequestHistories.Count().Should().Be(historyCount);
        await _hub.Received(2).NotifyRequestStatusChanged(request.PublicId.ToString(), RequestStatusConstants.Completed, "Tamamlandı", default);
    }

    [Fact]
    public async Task Notification_failure_does_not_undo_or_misreport_a_committed_close()
    {
        var request = Seed();
        _hub.NotifyRequestStatusChanged(Arg.Any<string>(), Arg.Any<Guid>(), Arg.Any<string>(), default)
            .Returns(Task.FromException(new InvalidOperationException("Hub unavailable")));
        var handler = new CloseRequestCommandHandler(Uow, _hub, _logger);
        (await handler.Handle(new() { PublicId = request.PublicId, CustomerId = request.CustomerId }, default)).Should().BeTrue();
        Db.ChangeTracker.Clear();
        Db.CustomerRequests.Find(request.Id)!.StatusId.Should().Be(RequestStatusConstants.Completed);
        Db.CustomerRequestEvaluations.Count().Should().Be(1);
        _logger.ReceivedCalls().Should().Contain(call => call.GetMethodInfo().Name == "Log"
            && (LogLevel)call.GetArguments()[0]! == LogLevel.Warning);
    }
}

using FluentAssertions;
using HelpCenter.Application.Exceptions;
using HelpCenter.Application.Features.Requests.Commands.MarkMessagesRead;
using HelpCenter.Application.Interfaces;
using HelpCenter.Application.Tests.Common;
using HelpCenter.Domain.Constants;
using HelpCenter.Domain.Entities;
using HelpCenter.Domain.Enums;
using HelpCenter.Persistence.Services;
using NSubstitute;
using Xunit;

namespace HelpCenter.Application.Tests.Features.Requests;

public class MarkMessagesReadTests : HandlerTestBase
{
    private readonly IUserContext _user = Substitute.For<IUserContext>();
    private readonly IDataScopeService _scope = Substitute.For<IDataScopeService>();
    private readonly IRequestHubService _hub = Substitute.For<IRequestHubService>();
    private readonly IConversationReadService _read = Substitute.For<IConversationReadService>();

    public MarkMessagesReadTests()
    {
        _user.IsAuthenticated.Returns(true);
        _user.UserId.Returns(TestIds.FromInt(7));
        _user.IsCustomer.Returns(true);
    }

    private CustomerRequest Seed(bool conversation = true)
    {
        var request = CustomerRequest.Create(TestIds.FromInt(7), null, null, "Read test", RequestPriority.Low);
        if (conversation) request.Conversation = new Conversation { Title = "Read test" };
        Db.CustomerRequests.Add(request);
        Db.SaveChanges();
        return request;
    }

    private MarkMessagesReadCommandHandler Handler(IConversationReadService? reader = null) =>
        new(Uow, reader ?? _read, _hub, _user, _scope);

    [Fact]
    public async Task Missing_request_returns_not_found_without_writes_or_notifications()
    {
        await Handler().Invoking(h => h.Handle(new() { RequestPublicId = Guid.NewGuid() }, default))
            .Should().ThrowAsync<RequestNotFoundException>();
        _read.ReceivedCalls().Should().BeEmpty();
        _hub.ReceivedCalls().Should().BeEmpty();
    }

    [Fact]
    public async Task Another_customers_request_is_not_mutated()
    {
        var request = Seed();
        _user.UserId.Returns(TestIds.FromInt(8));
        await Handler().Invoking(h => h.Handle(new() { RequestPublicId = request.PublicId }, default))
            .Should().ThrowAsync<RequestNotBelongToCustomerException>();
        _read.ReceivedCalls().Should().BeEmpty();
        _hub.ReceivedCalls().Should().BeEmpty();
    }

    [Fact]
    public async Task Unauthenticated_reader_is_rejected()
    {
        _user.IsAuthenticated.Returns(false);
        await Handler().Invoking(h => h.Handle(new() { RequestPublicId = Guid.NewGuid() }, default))
            .Should().ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task Customer_cannot_spoof_staff_reader()
    {
        var request = Seed();
        await Handler().Invoking(h => h.Handle(new() { RequestPublicId = request.PublicId, IsAgent = true }, default))
            .Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task No_conversation_is_a_successful_no_op()
    {
        var request = Seed(false);
        (await Handler().Handle(new() { RequestPublicId = request.PublicId }, default)).Should().BeTrue();
        _read.ReceivedCalls().Should().BeEmpty();
        _hub.ReceivedCalls().Should().BeEmpty();
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(2, 1)]
    public async Task Notify_only_when_messages_changed(int count, int notifications)
    {
        var request = Seed();
        _read.MarkMessagesAsReadAsync(request.ConversationId!.Value, false, default).Returns(count);
        (await Handler().Handle(new() { RequestPublicId = request.PublicId }, default)).Should().BeTrue();
        await _hub.Received(notifications).NotifyMessagesRead(request.PublicId.ToString(), false, default);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, true)]
    public async Task Assigned_route_requires_read_permission_and_current_assignment(bool assigned, bool allowed)
    {
        var request = Seed();
        _user.IsCustomer.Returns(false);
        _scope.HasPermissionAsync(AppResources.AssignedRequests, PermissionActions.Read, default).Returns(true);
        request.CurrentExpertId = assigned ? TestIds.FromInt(7) : TestIds.FromInt(8);
        await Db.SaveChangesAsync();
        var action = () => Handler().Handle(new() { RequestPublicId = request.PublicId, IsAgent = true, OnlyAssignedToCurrentUser = true }, default);
        if (allowed) (await action()).Should().BeTrue();
        else await action.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task Staff_without_read_permission_is_rejected()
    {
        var request = Seed();
        _user.IsCustomer.Returns(false);
        await Handler().Invoking(h => h.Handle(new() { RequestPublicId = request.PublicId, IsAgent = true }, default))
            .Should().ThrowAsync<ForbiddenException>();
        _read.ReceivedCalls().Should().BeEmpty();
    }

    [Fact]
    public async Task Read_persists_only_public_staff_messages_and_repeat_does_not_notify()
    {
        var request = Seed();
        var conversation = request.Conversation!;
        var staff = conversation.AddParticipant(null, TestIds.FromInt(9), ParticipantType.Agent);
        var customer = conversation.AddParticipant(TestIds.FromInt(7), null, ParticipantType.Customer);
        var publicMessage = new CustomerRequestMessage { SenderParticipant = staff, Type = MessageType.Public, MessageText = "Public" };
        var internalNote = new CustomerRequestMessage { SenderParticipant = staff, Type = MessageType.InternalNote, MessageText = "Internal" };
        var ownMessage = new CustomerRequestMessage { SenderParticipant = customer, MessageText = "Own" };
        conversation.Messages.Add(publicMessage);
        conversation.Messages.Add(internalNote);
        conversation.Messages.Add(ownMessage);
        await Db.SaveChangesAsync();
        var publicId = publicMessage.Id;
        var noteId = internalNote.Id;
        var ownId = ownMessage.Id;
        Db.ChangeTracker.Clear();
        var handler = Handler(new ConversationReadService(Db));
        await handler.Handle(new() { RequestPublicId = request.PublicId }, default);
        await handler.Handle(new() { RequestPublicId = request.PublicId }, default);
        Db.ChangeTracker.Clear();
        Db.CustomerRequestMessages.Find(publicId)!.IsRead.Should().BeTrue();
        Db.CustomerRequestMessages.Find(noteId)!.IsRead.Should().BeFalse();
        Db.CustomerRequestMessages.Find(ownId)!.IsRead.Should().BeFalse();
        await _hub.Received(1).NotifyMessagesRead(request.PublicId.ToString(), false, default);
    }
}

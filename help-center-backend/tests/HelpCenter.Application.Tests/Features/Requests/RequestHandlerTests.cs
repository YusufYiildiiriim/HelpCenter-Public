using FluentAssertions;
using HelpCenter.Application.Exceptions;
using HelpCenter.Application.Features.Requests.Commands.AdminCloseRequest;
using HelpCenter.Application.Features.Requests.Commands.AdminSendMessage;
using HelpCenter.Application.Features.Requests.Commands.CloseRequest;
using HelpCenter.Application.Features.Requests.Commands.ConsultExpert;
using HelpCenter.Application.Features.Requests.Commands.CreateRequest;
using HelpCenter.Application.Features.Requests.Commands.MarkMessagesRead;
using HelpCenter.Application.Features.Requests.Commands.SendMessage;
using HelpCenter.Application.Interfaces;
using HelpCenter.Application.Tests.Common;
using HelpCenter.Domain.Constants;
using HelpCenter.Domain.Entities;
using HelpCenter.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;

namespace HelpCenter.Application.Tests.Features.Requests;

public class RequestHandlerTests : HandlerTestBase
{
    private readonly IFileService _fileService = Substitute.For<IFileService>();
    private readonly IPublisher _publisher = Substitute.For<IPublisher>();
    private readonly IRequestHubService _hubService = Substitute.For<IRequestHubService>();

    private Customer SeedCustomer()
    {
        var company = new Company { Name = "CustomerCo_" + Guid.NewGuid().ToString("N")[..8], CreatedAt = DateTime.UtcNow };
        Db.Companies.Add(company);
        Db.SaveChanges();

        var account = new Account
        {
            Email = "customer@example.com",
            Username = "customer.user",
            FirstName = "Customer",
            LastName = "Test",
            Password = "pwd",
            CreatedAt = DateTime.UtcNow
        };
        var customer = new Customer
        {
            CompanyId = company.Id,
            Company = company,
            Account = account,
            CreatedAt = DateTime.UtcNow
        };
        Db.Accounts.Add(account);
        Db.Customers.Add(customer);
        Db.SaveChanges();
        Db.ChangeTracker.Clear();
        return customer;
    }

    private CustomerRequest SeedRequest(Customer customer)
    {
        var req = CustomerRequest.Create(customer.Id, null, null, "Bug Report", RequestPriority.High, "Customer Test");
        req.TicketId = "TCK-1001";
        Db.CustomerRequests.Add(req);
        Db.SaveChanges();

        var conv = new Conversation { Title = req.Title };
        Db.Conversations.Add(conv);
        Db.SaveChanges();

        req.ConversationId = conv.Id;
        req.Conversation = conv;
        Db.SaveChanges();
        Db.ChangeTracker.Clear();
        return req;
    }

    [Fact]
    public async Task CreateRequest_should_create_request_and_initial_message()
    {
        var customer = SeedCustomer();

        var createRules = new CreateRequestRules(Uow);
        var handler = new CreateRequestCommandHandler(
            Uow, createRules, _fileService, _publisher,
            NullLogger<CreateRequestCommandHandler>.Instance);

        var command = new CreateRequestCommand
        {
            CustomerId = customer.Id,
            Title = "Need help with integration",
            MessageText = "Initial message details",
            Priority = RequestPriority.Medium
        };

        var result = await handler.Handle(command, CancellationToken.None);

        result.TicketId.Should().NotBeNullOrEmpty();
        result.PublicId.Should().NotBeEmpty();

        var saved = Db.CustomerRequests.FirstOrDefault(r => r.CustomerId == customer.Id);
        saved.Should().NotBeNull();
        saved!.Title.Should().Be("Need help with integration");
        saved.ConversationId.Should().NotBeNull();
        result.PublicId.Should().Be(saved.PublicId);
    }

    [Fact]
    public async Task CreateRequest_with_allowed_module_should_succeed()
    {
        var customer = SeedCustomer();
        var module = new Module { Name = "Billing", IsActive = true, CreatedAt = DateTime.UtcNow };
        Db.Modules.Add(module);
        await Db.SaveChangesAsync();

        Db.CompanyModules.Add(new CompanyModule { CompanyId = customer.CompanyId, ModuleId = module.Id, IsActive = true });
        await Db.SaveChangesAsync();
        Db.ChangeTracker.Clear();

        var createRules = new CreateRequestRules(Uow);
        var handler = new CreateRequestCommandHandler(
            Uow, createRules, _fileService, _publisher,
            NullLogger<CreateRequestCommandHandler>.Instance);

        var command = new CreateRequestCommand
        {
            CustomerId = customer.Id,
            ModuleId = module.Id,
            Title = "Billing Question",
            MessageText = "How to invoice?",
            Priority = RequestPriority.Low
        };

        var result = await handler.Handle(command, CancellationToken.None);
        result.TicketId.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task CreateRequest_with_unallowed_module_should_throw_ModuleNotAllowedForCompanyException()
    {
        var customer = SeedCustomer();
        var unallowedModule = new Module { Name = "UnallowedModule", IsActive = true, CreatedAt = DateTime.UtcNow };
        Db.Modules.Add(unallowedModule);
        await Db.SaveChangesAsync();
        Db.ChangeTracker.Clear();

        var createRules = new CreateRequestRules(Uow);
        var handler = new CreateRequestCommandHandler(
            Uow, createRules, _fileService, _publisher,
            NullLogger<CreateRequestCommandHandler>.Instance);

        var command = new CreateRequestCommand
        {
            CustomerId = customer.Id,
            ModuleId = unallowedModule.Id,
            Title = "Unauthorized module request",
            MessageText = "Should fail",
            Priority = RequestPriority.Low
        };

        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<ModuleNotAllowedForCompanyException>();
    }

    [Fact]
    public async Task SendMessage_should_add_message_and_notify_hub()
    {
        var customer = SeedCustomer();
        var req = SeedRequest(customer);

        var handler = new SendMessageCommandHandler(
            Uow, _fileService, _hubService,
            NullLogger<SendMessageCommandHandler>.Instance);

        var command = new SendMessageCommand
        {
            CustomerId = customer.Id,
            RequestPublicId = req.PublicId,
            MessageText = "Follow up message"
        };

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().BeTrue();
        Db.CustomerRequestMessages.Should().ContainSingle(m => m.MessageText == "Follow up message");
        await _hubService.Received(1).NotifyNewMessage(req.PublicId.ToString(), Arg.Any<object>(), Arg.Any<MessageType>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AdminSendMessage_should_add_admin_message()
    {
        var customer = SeedCustomer();
        var req = SeedRequest(customer);

        var account = new Account { Email = "agent@test.com", Username = "agent", FirstName = "Admin", LastName = "User", Password = "p" };
        var agentUser = new User { Account = account, IsActive = true };
        Db.Accounts.Add(account);
        Db.Users.Add(agentUser);
        Db.SaveChanges();
        Db.ChangeTracker.Clear();

        var handler = new AdminSendMessageCommandHandler(
            Uow, _hubService, _fileService);

        var command = new AdminSendMessageCommand
        {
            SenderUserId = agentUser.Id,
            RequestPublicId = req.PublicId,
            MessageText = "Admin reply to customer",
            Type = MessageType.Public
        };

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().BeTrue();
        Db.CustomerRequestMessages.Should().ContainSingle(m => m.MessageText == "Admin reply to customer");

        var sentMessage = Db.CustomerRequestMessages.First(m => m.MessageText == "Admin reply to customer");
        var senderParticipant = Db.ConversationParticipants.Find(sentMessage.SenderParticipantId);
        senderParticipant!.Type.Should().Be(ParticipantType.Agent);
    }

    [Fact]
    public async Task AdminSendMessage_should_mark_sender_as_expert_when_sender_is_the_requests_current_expert()
    {
        // The same staff message endpoint is used both by regular agents and by experts
        // assigned to the ticket — the participant type should be determined by the
        // CustomerRequest.CurrentExpertId match, not by the role name.
        var customer = SeedCustomer();
        var req = SeedRequest(customer);

        var account = new Account { Email = "expertreply@test.com", Username = "expertreply", FirstName = "Exp", LastName = "Reply", Password = "p" };
        var expertUser = new User { Account = account, IsActive = true };
        Db.Accounts.Add(account);
        Db.Users.Add(expertUser);
        Db.SaveChanges();

        req.CurrentExpertId = expertUser.Id;
        Db.CustomerRequests.Update(req);
        Db.SaveChanges();
        Db.ChangeTracker.Clear();

        var handler = new AdminSendMessageCommandHandler(Uow, _hubService, _fileService);

        var command = new AdminSendMessageCommand
        {
            SenderUserId = expertUser.Id,
            RequestPublicId = req.PublicId,
            MessageText = "Expert reply to customer",
            Type = MessageType.Public
        };

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().BeTrue();
        var sentMessage = Db.CustomerRequestMessages.First(m => m.MessageText == "Expert reply to customer");
        var senderParticipant = Db.ConversationParticipants.Find(sentMessage.SenderParticipantId);
        senderParticipant!.Type.Should().Be(ParticipantType.Expert);
    }

    [Fact]
    public async Task CloseRequest_should_update_status_to_closed()
    {
        var customer = SeedCustomer();
        var req = SeedRequest(customer);

        var handler = new CloseRequestCommandHandler(Uow);
        var command = new CloseRequestCommand
        {
            CustomerId = customer.Id,
            PublicId = req.PublicId
        };

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().BeTrue();
        var updated = Db.CustomerRequests.Find(req.Id);
        updated!.StatusId.Should().Be(RequestStatusConstants.Completed);
    }

    [Fact]
    public async Task ConsultExpert_should_add_consulted_expert_to_request()
    {
        var customer = SeedCustomer();
        var req = SeedRequest(customer);

        var account = new Account { Email = "expert@test.com", Username = "expert", FirstName = "Exp", LastName = "E", Password = "p" };
        var expertUser = new User { Account = account, IsActive = true };
        Db.Accounts.Add(account);
        Db.Users.Add(expertUser);

        var agentAccount = new Account { Email = "agent2@test.com", Username = "agent2", FirstName = "Ag", LastName = "T", Password = "p" };
        var agentUser = new User { Account = agentAccount, IsActive = true };
        Db.Accounts.Add(agentAccount);
        Db.Users.Add(agentUser);
        Db.SaveChanges();
        Db.ChangeTracker.Clear();

        var emailDispatcher = Substitute.For<IEmailDispatcher>();
        var handler = new ConsultExpertCommandHandler(
            Uow, _hubService, emailDispatcher);
        var command = new ConsultExpertCommand
        {
            RequestPublicId = req.PublicId,
            ExpertId = expertUser.Id,
            AgentUserId = agentUser.Id,
            Note = "Please check this issue"
        };

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().BeTrue();
        var updated = Db.CustomerRequests.Find(req.Id);
        updated!.CurrentExpertId.Should().Be(expertUser.Id);
        updated.StatusId.Should().Be(RequestStatusConstants.UnderTechnicalReview);
    }
}

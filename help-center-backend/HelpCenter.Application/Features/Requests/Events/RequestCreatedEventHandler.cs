using HelpCenter.Application.Interfaces;
using HelpCenter.Domain.Entities;
using HelpCenter.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HelpCenter.Application.Features.Requests.Events;

// MediatR adapter — domain event does not depend on MediatR directly
public record RequestCreatedNotification(RequestCreatedEvent DomainEvent) : INotification;

public class RequestCreatedEventHandler : INotificationHandler<RequestCreatedNotification>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailDispatcher _emailDispatcher;
    private readonly ILogger<RequestCreatedEventHandler> _logger;

    public RequestCreatedEventHandler(IUnitOfWork unitOfWork, IEmailDispatcher emailDispatcher, ILogger<RequestCreatedEventHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _emailDispatcher = emailDispatcher;
        _logger = logger;
    }

    public async Task Handle(RequestCreatedNotification notification, CancellationToken cancellationToken)
    {
        var evt = notification.DomainEvent;

        // Previously the full UserRole/User/Account graph was fetched via an Include on the
        // "User.Account" string path and (Email, FullName) extracted manually. Now only these
        // two fields are projected directly in SQL.
        var agentContacts = await _unitOfWork.Repository<UserRole>().SelectAsync(
            ur => ur.RoleId == 2 && ur.IsActive &&
                  ur.User != null && ur.User.IsActive && ur.User.Account != null,
            ur => new { ur.User!.Account.Email, ur.User.Account.FullName },
            cancellationToken: cancellationToken);

        var agentList = agentContacts.Select(a => (a.Email, a.FullName));

        // Writes to the queue — mail sending is done in the background by EmailBackgroundService
        _emailDispatcher.EnqueueAgentNotifications(evt.CustomerFullName, evt.RequestTitle, agentList);

        _logger.LogInformation("{Count} agent mail kuyruğuna eklendi. Talep: {Title}", agentContacts.Count, evt.RequestTitle);
    }
}

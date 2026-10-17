using HelpCenter.Domain.Common;

namespace HelpCenter.Domain.Events;

public record RequestCreatedEvent(
    Guid CustomerId,
    string CustomerFullName,
    string RequestTitle
) : IDomainEvent;

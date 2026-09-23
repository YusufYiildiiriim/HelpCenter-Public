using HelpCenter.Domain.Common;

namespace HelpCenter.Domain.Events;

public record RequestCreatedEvent(
    int CustomerId,
    string CustomerFullName,
    string RequestTitle
) : IDomainEvent;

using HelpCenter.Domain.Common;

namespace HelpCenter.Domain.Events;

public record ExpertAssignedEvent(
    string ExpertEmail,
    string ExpertFullName,
    string AgentFullName,
    string RequestTitle
) : IDomainEvent;

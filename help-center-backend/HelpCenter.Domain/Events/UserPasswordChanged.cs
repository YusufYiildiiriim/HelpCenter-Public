using HelpCenter.Domain.Common;

namespace HelpCenter.Domain.Events;

public sealed record UserPasswordChanged(Guid UserId, DateTime ChangedAt) : IDomainEvent;

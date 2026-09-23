using HelpCenter.Domain.Common;

namespace HelpCenter.Domain.Events;

public sealed record UserPasswordChanged(int UserId, DateTime ChangedAt) : IDomainEvent;

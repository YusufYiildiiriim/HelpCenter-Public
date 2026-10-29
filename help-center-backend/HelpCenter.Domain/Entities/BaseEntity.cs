using HelpCenter.Domain.Common;

namespace HelpCenter.Domain.Entities;

public abstract class BaseEntity : IAuditable, ISoftDelete
{
    protected BaseEntity()
    {
        Id = Guid.NewGuid();
        PublicId = Id;
    }

    public Guid Id { get; set; }
    public Guid PublicId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
    public bool IsActive { get; set; } = true;

    public Guid? CreatedByAccountId { get; set; }
    public Guid? LastModifiedByAccountId { get; set; }

    public Guid RowVersion { get; set; } = Guid.NewGuid();

    private readonly List<IDomainEvent> _domainEvents = [];

    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void RaiseDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);

    public void ClearDomainEvents() => _domainEvents.Clear();

    public virtual void MarkAsDeleted()
    {
        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
    }
}

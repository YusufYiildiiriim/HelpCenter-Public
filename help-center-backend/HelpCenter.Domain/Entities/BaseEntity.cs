using HelpCenter.Domain.Common;

namespace HelpCenter.Domain.Entities;

public abstract class BaseEntity : IAuditable, ISoftDelete
{
    public int Id { get; set; }
    public Guid PublicId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
    public bool IsActive { get; set; } = true;

    public int? CreatedByAccountId { get; set; }
    public int? LastModifiedByAccountId { get; set; }

    public byte[] RowVersion { get; set; } = new byte[8];

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

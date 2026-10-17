namespace HelpCenter.Domain.Common;

public interface IAuditable
{
    DateTime CreatedAt { get; set; }
    Guid? CreatedByAccountId { get; set; }
    DateTime? UpdatedAt { get; set; }
    Guid? LastModifiedByAccountId { get; set; }
}

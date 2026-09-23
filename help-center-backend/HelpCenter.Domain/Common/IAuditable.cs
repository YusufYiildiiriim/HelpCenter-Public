namespace HelpCenter.Domain.Common;

public interface IAuditable
{
    DateTime CreatedAt { get; set; }
    int? CreatedByAccountId { get; set; }
    DateTime? UpdatedAt { get; set; }
    int? LastModifiedByAccountId { get; set; }
}

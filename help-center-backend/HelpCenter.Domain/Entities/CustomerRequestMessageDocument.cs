namespace HelpCenter.Domain.Entities;

public class CustomerRequestMessageDocument : BaseEntity
{
    public string Path { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public Guid CustomerRequestId { get; set; }
    public virtual CustomerRequest CustomerRequest { get; set; } = null!;
    public Guid? CustomerRequestMessageId { get; set; }
    public virtual CustomerRequestMessage? CustomerRequestMessage { get; set; }
}


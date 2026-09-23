using HelpCenter.Domain.Enums;

namespace HelpCenter.Application.Features.Requests.Queries.GetCustomerMessages;

public class CustomerMessageDto
{
    public int Id { get; set; }
    public string MessageText { get; set; } = string.Empty;
    public bool IsAgent { get; set; }
    public DateTime CreatedAt { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public MessageType Type { get; set; }
    public List<CustomerMessageDocumentDto> Documents { get; set; } = new();
}

public class CustomerMessageDocumentDto
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
}

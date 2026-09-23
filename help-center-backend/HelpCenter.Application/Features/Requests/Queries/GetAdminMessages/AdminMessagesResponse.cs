using HelpCenter.Domain.Enums;

namespace HelpCenter.Application.Features.Requests.Queries.GetAdminMessages;

public class AdminMessagesResponse
{
    public List<AdminMessageDto> Messages { get; set; } = new();
    public bool HasMore { get; set; }
    public int TotalCount { get; set; }
}

public class AdminMessageDto
{
    public int Id { get; set; }
    public string MessageText { get; set; } = string.Empty;
    public bool IsAgent { get; set; }
    public DateTime CreatedAt { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public MessageType Type { get; set; }
    public List<AdminMessageDocumentDto> Documents { get; set; } = new();
}

public class AdminMessageDocumentDto
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
}

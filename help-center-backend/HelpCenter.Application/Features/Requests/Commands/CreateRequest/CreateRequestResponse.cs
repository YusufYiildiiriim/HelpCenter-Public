namespace HelpCenter.Application.Features.Requests.Commands.CreateRequest;

public class CreateRequestResponse
{
    public string TicketId { get; set; } = string.Empty;
    public Guid PublicId { get; set; }
}

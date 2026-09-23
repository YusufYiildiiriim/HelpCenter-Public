using MediatR;

namespace HelpCenter.Application.Features.Requests.Commands.CloseRequest;

public class CloseRequestCommand : IRequest<bool>
{
    public Guid PublicId { get; set; }
    public int CustomerId { get; set; }
    public string? Note { get; set; }
    public int Rating { get; set; } = 5;
}

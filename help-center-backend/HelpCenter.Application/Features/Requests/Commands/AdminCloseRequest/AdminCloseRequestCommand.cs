using MediatR;

namespace HelpCenter.Application.Features.Requests.Commands.AdminCloseRequest;

public class AdminCloseRequestCommand : IRequest<bool>
{
    public int Id { get; set; }
    public string? Note { get; set; }
    public int AgentUserId { get; set; }
}

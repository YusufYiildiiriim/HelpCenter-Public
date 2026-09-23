using MediatR;

namespace HelpCenter.Application.Features.Requests.Commands.ConsultExpert;

public class ConsultExpertCommand : IRequest<bool>
{
    public Guid RequestPublicId { get; set; }
    public int ExpertId { get; set; }
    public int AgentUserId { get; set; }
    public string Note { get; set; } = string.Empty;
}

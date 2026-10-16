using HelpCenter.Application.Common.Caching;
using MediatR;

namespace HelpCenter.Application.Features.Requests.Commands.ConsultExpert;

public class ConsultExpertCommand : IRequest<bool>, IInvalidatesMessageCache
{
    public Guid RequestPublicId { get; set; }
    public int ExpertId { get; set; }
    public int AgentUserId { get; set; }
    public string Note { get; set; } = string.Empty;
}

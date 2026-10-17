using HelpCenter.Application.Common.Caching;
using MediatR;

namespace HelpCenter.Application.Features.Requests.Commands.CloseRequest;

public class CloseRequestCommand : IRequest<bool>, IInvalidatesMessageCache
{
    public Guid PublicId { get; set; }
    public Guid CustomerId { get; set; }
    public string? Note { get; set; }
    public int Rating { get; set; } = 5;
}

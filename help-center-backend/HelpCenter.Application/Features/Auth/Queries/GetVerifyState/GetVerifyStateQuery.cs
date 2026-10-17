using MediatR;

namespace HelpCenter.Application.Features.Auth.Queries.GetVerifyState;

public record GetVerifyStateQuery(Guid UserId) : IRequest<GetVerifyStateResponse>;

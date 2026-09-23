using MediatR;

namespace HelpCenter.Application.Features.Auth.Queries.GetVerifyState;

public record GetVerifyStateQuery(int UserId) : IRequest<GetVerifyStateResponse>;

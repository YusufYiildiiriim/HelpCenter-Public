using HelpCenter.Application.Features.Requests.Queries.GetAdminRequests;
using MediatR;

namespace HelpCenter.Application.Features.Requests.Queries.GetAdminRequestById;

public record GetAdminRequestByIdQuery(
    Guid PublicId,
    bool OnlyAssignedToCurrentUser = false) : IRequest<AdminRequestDto>;

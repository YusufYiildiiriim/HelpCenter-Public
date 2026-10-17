using MediatR;

namespace HelpCenter.Application.Features.Projects.Queries.GetUsersByProjectId;

public record GetUsersByProjectIdQuery(Guid ProjectId) : IRequest<List<ProjectUserDto>>;

using MediatR;

namespace HelpCenter.Application.Features.Projects.Queries.GetUsersByProjectId;

public record GetUsersByProjectIdQuery(int ProjectId) : IRequest<List<ProjectUserDto>>;

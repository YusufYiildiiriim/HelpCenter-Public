using MediatR;

namespace HelpCenter.Application.Features.Projects.Queries.GetPublicProjects;

public record GetPublicProjectsQuery() : IRequest<List<PublicProjectDto>>;

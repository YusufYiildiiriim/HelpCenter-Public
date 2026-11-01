using MediatR;

namespace HelpCenter.Application.Features.Projects.Queries.GetPublicProjects;

public record GetPublicProjectsQuery(string Locale = HelpCenter.Domain.Common.SupportedLocales.Default) : IRequest<List<PublicProjectDto>>;

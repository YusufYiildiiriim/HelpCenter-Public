using MediatR;

namespace HelpCenter.Application.Features.Modules.Queries.GetPublicModules;

public record GetPublicModulesQuery(int? ProjectId = null) : IRequest<List<PublicModuleDto>>;

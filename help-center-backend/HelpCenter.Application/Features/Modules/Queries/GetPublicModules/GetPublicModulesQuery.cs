using MediatR;

namespace HelpCenter.Application.Features.Modules.Queries.GetPublicModules;

public record GetPublicModulesQuery(Guid? ProjectId = null) : IRequest<List<PublicModuleDto>>;

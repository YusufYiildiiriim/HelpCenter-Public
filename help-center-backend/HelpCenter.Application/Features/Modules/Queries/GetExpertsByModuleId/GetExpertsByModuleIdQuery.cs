using MediatR;

namespace HelpCenter.Application.Features.Modules.Queries.GetExpertsByModuleId;

public record GetExpertsByModuleIdQuery(Guid ModuleId) : IRequest<List<ModuleExpertDto>>;

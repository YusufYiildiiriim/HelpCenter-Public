using MediatR;

namespace HelpCenter.Application.Features.Modules.Queries.GetExpertsByModuleId;

public record GetExpertsByModuleIdQuery(int ModuleId) : IRequest<List<ModuleExpertDto>>;

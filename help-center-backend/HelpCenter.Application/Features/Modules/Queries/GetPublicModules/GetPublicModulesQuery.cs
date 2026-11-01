using MediatR;

namespace HelpCenter.Application.Features.Modules.Queries.GetPublicModules;

public record GetPublicModulesQuery(Guid? ProjectId = null, string Locale = HelpCenter.Domain.Common.SupportedLocales.Default) : IRequest<List<PublicModuleDto>>;

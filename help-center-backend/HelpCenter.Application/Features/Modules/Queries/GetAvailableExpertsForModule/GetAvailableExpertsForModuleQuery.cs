using HelpCenter.Application.Common.Models;
using MediatR;

namespace HelpCenter.Application.Features.Modules.Queries.GetAvailableExpertsForModule;

public record GetAvailableExpertsForModuleQuery() : IRequest<List<LookupItem>>;

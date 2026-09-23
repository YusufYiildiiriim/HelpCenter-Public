using HelpCenter.Application.Common.Models;
using MediatR;

namespace HelpCenter.Application.Features.Projects.Queries.GetAvailableModulesForProject;

public record GetAvailableModulesForProjectQuery() : IRequest<List<LookupItem>>;

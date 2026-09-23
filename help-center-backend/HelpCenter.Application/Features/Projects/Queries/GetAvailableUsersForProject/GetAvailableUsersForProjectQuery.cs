using HelpCenter.Application.Common.Models;
using MediatR;

namespace HelpCenter.Application.Features.Projects.Queries.GetAvailableUsersForProject;

public record GetAvailableUsersForProjectQuery() : IRequest<List<LookupItem>>;

using MediatR;

namespace HelpCenter.Application.Features.Statistics.Queries.GetAdminStatistics;

public record GetAdminStatisticsQuery() : IRequest<AdminStatisticsDto>;

using MediatR;

namespace HelpCenter.Application.Features.Statistics.Queries.GetAdminReports;

public record GetAdminReportsQuery() : IRequest<AdminReportsDto>;

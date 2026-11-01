using HelpCenter.Application.Common.Models;
using HelpCenter.Application.Interfaces;
using HelpCenter.Domain.Entities;
using MediatR;

namespace HelpCenter.Application.Features.Statuses.Queries.GetAllStatuses;

public class GetAllStatusesQueryHandler : IRequestHandler<GetAllStatusesQuery, PaginatedResponse<StatusDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAllStatusesQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PaginatedResponse<StatusDto>> Handle(GetAllStatusesQuery request, CancellationToken cancellationToken)
    {
        var statuses = await _unitOfWork.Repository<CustomerRequestStatus>()
            .FindAsync(_ => true, cancellationToken);
        var search = request.Search?.Trim();
        var localized = statuses
            .Select(status =>
            {
                var text = StatusTextLocalizer.Localize(status.Id, status.Name, status.Description, request.Locale);
                return new StatusDto
                {
                    Id = status.Id,
                    Name = text.Name,
                    Description = text.Description,
                    IsActive = status.IsActive
                };
            })
            .Where(status => string.IsNullOrWhiteSpace(search)
                || status.Name.Contains(search, StringComparison.OrdinalIgnoreCase))
            .OrderBy(status => status.Id)
            .ToList();

        var pageNumber = Math.Max(1, request.PageNumber);
        var pageSize = request.PageSize <= 0 ? Math.Max(localized.Count, 1) : request.PageSize;
        var totalCount = localized.Count;
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        return new PaginatedResponse<StatusDto>
        {
            Items = localized.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList(),
            TotalCount = totalCount,
            TotalPages = totalPages,
            CurrentPage = pageNumber,
            PageSize = pageSize
        };
    }
}

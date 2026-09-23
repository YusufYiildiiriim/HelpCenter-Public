using AutoMapper;
using HelpCenter.Application.Common.Models;
using HelpCenter.Application.Interfaces;
using HelpCenter.Domain.Entities;
using MediatR;

namespace HelpCenter.Application.Features.Users.Queries.GetUsers;

public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, PaginatedResponse<UserDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetUsersQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PaginatedResponse<UserDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var search = request.Search?.Trim().ToLower();

        // Previously ALL matching users were pulled into memory with FindWithIncludesAsync and
        // sorted + paginated there (fake pagination) — now it's a single query using ProjectTo
        // (leveraging the existing UserProfile AutoMapper profile) + DB-level ordering/Skip/Take.
        return await _unitOfWork.Repository<User>().GetPaginatedProjectedAsync<UserDto>(
            x => string.IsNullOrWhiteSpace(search) ||
                 x.Account.FirstName.ToLower().Contains(search) ||
                 x.Account.LastName.ToLower().Contains(search) ||
                 x.Account.Email.ToLower().Contains(search),
            _mapper.ConfigurationProvider,
            request.PageNumber,
            request.PageSize,
            orderByDescendingKey: x => x.CreatedAt,
            cancellationToken: cancellationToken);
    }
}

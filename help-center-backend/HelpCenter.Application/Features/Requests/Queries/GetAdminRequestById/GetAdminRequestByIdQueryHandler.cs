using AutoMapper;
using HelpCenter.Application.Features.Requests.Queries.GetAdminRequests;
using HelpCenter.Application.Interfaces;
using HelpCenter.Domain.Entities;
using MediatR;

namespace HelpCenter.Application.Features.Requests.Queries.GetAdminRequestById;

public class GetAdminRequestByIdQueryHandler : IRequestHandler<GetAdminRequestByIdQuery, AdminRequestDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IUserContext _userContext;

    public GetAdminRequestByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, IUserContext userContext)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _userContext = userContext;
    }

    public async Task<AdminRequestDto> Handle(GetAdminRequestByIdQuery request, CancellationToken cancellationToken)
    {
        // Instead of fetching the full entity with string-path Includes ("Customer.Account", "Module", ...)
        // and then mapping with _mapper.Map(...), we translate the existing AdminRequestProfile (the
        // AutoMapper profile) directly into a SQL projection via ProjectTo — EF Core derives the
        // required joins itself from the MapFrom expressions in the profile. The mapping logic is still
        // defined in ONE place (AdminRequestProfile) and is not duplicated here.
        var dto = await _unitOfWork.Repository<CustomerRequest>().FirstOrDefaultProjectedAsync<AdminRequestDto>(
            x => x.PublicId == request.PublicId &&
                (!request.OnlyAssignedToCurrentUser ||
                 x.AssignedUserId == _userContext.UserId ||
                 x.CurrentExpertId == _userContext.UserId),
            _mapper.ConfigurationProvider,
            cancellationToken);

        GetAdminRequestByIdRules.RequestShouldExist(dto);

        return dto!;
    }
}

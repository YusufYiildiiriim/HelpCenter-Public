using System.Linq.Expressions;
using AutoMapper;
using HelpCenter.Application.Interfaces;
using HelpCenter.Domain.Entities;
using MediatR;

namespace HelpCenter.Application.Features.Customers.Queries.GetProfile;

public class GetProfileQueryHandler : IRequestHandler<GetProfileQuery, CustomerProfileDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetProfileQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<CustomerProfileDto> Handle(GetProfileQuery request, CancellationToken cancellationToken)
    {
        var entity = await _unitOfWork.Repository<Customer>().FirstOrDefaultAsync(
            x => x.Id == request.CustomerId && !x.IsDeleted,
            asTracking: false,
            cancellationToken: cancellationToken,
            x => x.Company, x => x.Account);

        GetProfileRules.ProfileShouldExist(entity);

        var dto = _mapper.Map<CustomerProfileDto>(entity);
        dto.PublicId = entity!.PublicId;

        return dto;
    }
}

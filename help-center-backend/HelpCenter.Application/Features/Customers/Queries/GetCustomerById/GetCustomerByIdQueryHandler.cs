using System.Linq.Expressions;
using AutoMapper;
using HelpCenter.Application.Features.Customers.Queries.GetCustomers;
using HelpCenter.Application.Interfaces;
using HelpCenter.Domain.Entities;
using MediatR;

namespace HelpCenter.Application.Features.Customers.Queries.GetCustomerById;

public class GetCustomerByIdQueryHandler : IRequestHandler<GetCustomerByIdQuery, CustomerDetailDto?>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetCustomerByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<CustomerDetailDto?> Handle(GetCustomerByIdQuery request, CancellationToken cancellationToken)
    {
        var customer = await _unitOfWork.Repository<Customer>().FirstOrDefaultAsync(
            x => x.PublicId == request.PublicId,
            asTracking: false,
            cancellationToken: cancellationToken,
            x => x.Company, x => x.Account);

        if (customer == null) return null;

        var dto = _mapper.Map<CustomerDetailDto>(customer);
        dto.PublicId = customer.PublicId;

        return dto;
    }
}

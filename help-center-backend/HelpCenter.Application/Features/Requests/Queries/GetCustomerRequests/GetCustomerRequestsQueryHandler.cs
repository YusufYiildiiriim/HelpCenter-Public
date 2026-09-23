using AutoMapper;
using HelpCenter.Application.Interfaces;
using HelpCenter.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HelpCenter.Application.Features.Requests.Queries.GetCustomerRequests;

public class GetCustomerRequestsQueryHandler : IRequestHandler<GetCustomerRequestsQuery, List<CustomerRequestDto>>
{
    private readonly IUnitOfWork _unitofwork;
    private readonly IMapper _mapper;
    private readonly ILogger<GetCustomerRequestsQueryHandler> _logger;

    public GetCustomerRequestsQueryHandler(IUnitOfWork unitofwork, IMapper mapper, ILogger<GetCustomerRequestsQueryHandler> logger)
    {
        _unitofwork = unitofwork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<List<CustomerRequestDto>> Handle(GetCustomerRequestsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Müşteri talepleri listeleniyor. Müşteri ID: {CustomerId}, Filtre: {Status}", request.CustomerId, request.Status);

        var requests = await _unitofwork.Repository<CustomerRequest>().FindAsync(
            x => x.CustomerId == request.CustomerId &&
                 (string.IsNullOrEmpty(request.Status) || x.Status.Name == request.Status),
            cancellationToken,
            x => x.Module!,
            x => x.Status,
            x => x.RequestSubject!
        );

        var dtoList = _mapper.Map<List<CustomerRequestDto>>(requests);

        _logger.LogInformation("Müşteri talepleri başarıyla listelendi. Toplam: {Count}", dtoList.Count);

        return dtoList;
    }
}

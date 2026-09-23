using System.Linq.Expressions;
using AutoMapper;
using HelpCenter.Application.Features.Requests.Queries.GetCustomerRequests;
using HelpCenter.Application.Interfaces;
using HelpCenter.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HelpCenter.Application.Features.Requests.Queries.GetCustomerRequestById;

public class GetCustomerRequestByIdQueryHandler : IRequestHandler<GetCustomerRequestByIdQuery, CustomerRequestDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<GetCustomerRequestByIdQueryHandler> _logger;

    public GetCustomerRequestByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, ILogger<GetCustomerRequestByIdQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<CustomerRequestDto> Handle(GetCustomerRequestByIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Talep detayı getiriliyor. PublicId: {PublicId}, Müşteri ID: {CustomerId}", request.PublicId, request.CustomerId);

        var entity = await _unitOfWork.Repository<CustomerRequest>().FirstOrDefaultAsync(
            x => x.PublicId == request.PublicId && x.CustomerId == request.CustomerId && !x.IsDeleted,
            asTracking: false,
            cancellationToken: cancellationToken,
            x => x.Module!, x => x.Status, x => x.RequestSubject!);

        if (entity == null)
        {
            _logger.LogWarning("Talep bulunamadı veya yetkisiz erişim. PublicId: {PublicId}, Müşteri ID: {CustomerId}", request.PublicId, request.CustomerId);
        }

        GetCustomerRequestByIdRules.RequestShouldExistForCustomer(entity);

        var dto = _mapper.Map<CustomerRequestDto>(entity);
        dto.PublicId = request.PublicId;

        _logger.LogInformation("Talep detayı başarıyla getirildi. TicketId: {TicketId}", entity!.TicketId);

        return dto;
    }
}

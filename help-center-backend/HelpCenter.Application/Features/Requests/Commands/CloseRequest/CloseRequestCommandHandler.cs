using HelpCenter.Application.Interfaces;
using HelpCenter.Domain.Entities;
using HelpCenter.Domain.Constants;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HelpCenter.Application.Features.Requests.Commands.CloseRequest;

public class CloseRequestCommandHandler : IRequestHandler<CloseRequestCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRequestHubService _hubService;
    private readonly ILogger<CloseRequestCommandHandler> _logger;

    public CloseRequestCommandHandler(IUnitOfWork unitOfWork, IRequestHubService hubService, ILogger<CloseRequestCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _hubService = hubService;
        _logger = logger;
    }

    public async Task<bool> Handle(CloseRequestCommand request, CancellationToken cancellationToken)
    {
        var customerRequest = (await _unitOfWork.Repository<CustomerRequest>()
            .FindAsync(x => x.PublicId == request.PublicId, cancellationToken, x => x.Customer)).FirstOrDefault();

        CloseRequestRules.RequestShouldExist(customerRequest);
        CloseRequestRules.RequestShouldBelongToCustomer(customerRequest!, request.CustomerId);

        // A retried close must not add another evaluation or history entry.
        if (customerRequest!.StatusId == RequestStatusConstants.Completed)
        {
            await NotifyClosedAsync(customerRequest, cancellationToken);
            return true;
        }

        var oldStatus = await _unitOfWork.Repository<CustomerRequestStatus>().GetAsync(customerRequest!.StatusId, cancellationToken);

        var evaluation = new CustomerRequestEvaluation
        {
            CustomerRequestId = customerRequest.Id,
            Note = request.Note ?? string.Empty,
            OldStatus = oldStatus?.Name ?? "Bilinmiyor",
            NewStatus = "Tamamlandı",
            CustomerUserId = request.CustomerId,
            CustomerName = $"{customerRequest.Customer.FirstName} {customerRequest.Customer.LastName}".Trim(),
            Rating = request.Rating
        };

        customerRequest.Complete(customerRequest.Customer.AccountId, request.Note);

        await _unitOfWork.Repository<CustomerRequest>().UpdateAsync(customerRequest, cancellationToken);
        await _unitOfWork.Repository<CustomerRequestEvaluation>().AddAsync(evaluation, cancellationToken);
        await _unitOfWork.SaveAsync(cancellationToken);

        await NotifyClosedAsync(customerRequest, cancellationToken);

        return true;
    }

    private async Task NotifyClosedAsync(CustomerRequest request, CancellationToken cancellationToken)
    {
        try
        {
            await _hubService.NotifyRequestStatusChanged(request.PublicId.ToString(), request.StatusId, "Tamamlandı", cancellationToken);
        }
        catch (Exception exception)
        {
            // The close is already committed. Do not misreport a failed close or
            // cause duplicate evaluations when a transient hub failure is retried.
            _logger.LogWarning(exception, "Closed request {RequestPublicId}, but its status notification failed.", request.PublicId);
        }
    }
}

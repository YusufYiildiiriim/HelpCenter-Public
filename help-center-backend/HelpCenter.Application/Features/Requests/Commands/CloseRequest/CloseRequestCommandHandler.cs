using HelpCenter.Application.Interfaces;
using HelpCenter.Domain.Entities;
using MediatR;

namespace HelpCenter.Application.Features.Requests.Commands.CloseRequest;

public class CloseRequestCommandHandler : IRequestHandler<CloseRequestCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public CloseRequestCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(CloseRequestCommand request, CancellationToken cancellationToken)
    {
        var customerRequest = (await _unitOfWork.Repository<CustomerRequest>()
            .FindAsync(x => x.PublicId == request.PublicId, cancellationToken, x => x.Customer)).FirstOrDefault();

        CloseRequestRules.RequestShouldExist(customerRequest);
        CloseRequestRules.RequestShouldBelongToCustomer(customerRequest!, request.CustomerId);

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

        return true;
    }
}

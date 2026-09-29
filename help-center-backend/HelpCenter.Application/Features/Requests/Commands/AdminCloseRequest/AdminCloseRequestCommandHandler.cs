using HelpCenter.Application.Interfaces;
using HelpCenter.Domain.Entities;
using MediatR;

namespace HelpCenter.Application.Features.Requests.Commands.AdminCloseRequest;

public class AdminCloseRequestCommandHandler : IRequestHandler<AdminCloseRequestCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserContext _userContext;

    public AdminCloseRequestCommandHandler(IUnitOfWork unitOfWork, IUserContext userContext)
    {
        _unitOfWork = unitOfWork;
        _userContext = userContext;
    }

    public async Task<bool> Handle(AdminCloseRequestCommand request, CancellationToken cancellationToken)
    {
        var customerRequest = await _unitOfWork.Repository<CustomerRequest>().GetAsync(request.Id, cancellationToken);
        AdminCloseRequestRules.RequestShouldExist(customerRequest);

        var oldStatus = await _unitOfWork.Repository<CustomerRequestStatus>().GetAsync(customerRequest!.StatusId, cancellationToken);
        var actor = (await _unitOfWork.Repository<User>().FindAsync(
            x => x.Id == _userContext.UserId,
            cancellationToken,
            x => x.Account)).FirstOrDefault();
        AdminCloseRequestRules.ActorShouldExist(actor);

        var evaluation = new CustomerRequestEvaluation
        {
            CustomerRequestId = customerRequest.Id,
            Note = request.Note ?? "Admin tarafından kapatıldı.",
            OldStatus = oldStatus?.Name ?? "Bilinmiyor",
            NewStatus = "Tamamlandı",
            CustomerUserId = actor!.Id,
            CustomerName = $"{actor.FirstName} {actor.LastName}",
            Rating = 5
        };

        customerRequest.Complete(actor.AccountId, request.Note ?? "Admin tarafından kapatıldı.");

        await _unitOfWork.Repository<CustomerRequestEvaluation>().AddAsync(evaluation, cancellationToken);
        await _unitOfWork.SaveAsync(cancellationToken);

        return true;
    }
}

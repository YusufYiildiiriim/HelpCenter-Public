using HelpCenter.Application.Interfaces;
using HelpCenter.Domain.Entities;
using MediatR;

namespace HelpCenter.Application.Features.Requests.Commands.AdminCloseRequest;

public class AdminCloseRequestCommandHandler : IRequestHandler<AdminCloseRequestCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public AdminCloseRequestCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(AdminCloseRequestCommand request, CancellationToken cancellationToken)
    {
        var customerRequest = await _unitOfWork.Repository<CustomerRequest>().GetAsync(request.Id, cancellationToken);
        AdminCloseRequestRules.RequestShouldExist(customerRequest);

        var oldStatus = await _unitOfWork.Repository<CustomerRequestStatus>().GetAsync(customerRequest!.StatusId, cancellationToken);
        var agent = (await _unitOfWork.Repository<User>().FindAsync(x => x.Id == request.AgentUserId, cancellationToken, x => x.Account)).FirstOrDefault();

        var evaluation = new CustomerRequestEvaluation
        {
            CustomerRequestId = customerRequest.Id,
            Note = request.Note ?? "Admin tarafından kapatıldı.",
            OldStatus = oldStatus?.Name ?? "Bilinmiyor",
            NewStatus = "Tamamlandı",
            CustomerUserId = request.AgentUserId,
            CustomerName = agent != null ? $"{agent.FirstName} {agent.LastName}" : "Sistem",
            Rating = 5
        };

        int actorAccountId = agent?.AccountId ?? 1;
        customerRequest.Complete(actorAccountId, request.Note ?? "Admin tarafından kapatıldı.");

        await _unitOfWork.Repository<CustomerRequestEvaluation>().AddAsync(evaluation, cancellationToken);
        await _unitOfWork.SaveAsync(cancellationToken);

        return true;
    }
}

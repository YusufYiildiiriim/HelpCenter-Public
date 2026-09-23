using HelpCenter.Application.Interfaces;
using HelpCenter.Domain.Entities;
using MediatR;

namespace HelpCenter.Application.Features.Companies.Commands.DeleteCompany;

public class DeleteCompanyCommandHandler : IRequestHandler<DeleteCompanyCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly DeleteCompanyRules _rules;

    public DeleteCompanyCommandHandler(IUnitOfWork unitOfWork, DeleteCompanyRules rules)
    {
        _unitOfWork = unitOfWork;
        _rules = rules;
    }

    public async Task<bool> Handle(DeleteCompanyCommand request, CancellationToken cancellationToken)
    {
        var company = await _unitOfWork.Repository<Company>().FirstOrDefaultAsync(
            x => x.PublicId == request.PublicId, asTracking: true, cancellationToken: cancellationToken);
        DeleteCompanyRules.CompanyShouldExist(company);

        await _rules.CompanyShouldNotHaveActiveRequestsAsync(company!.Id, cancellationToken);

        company!.MarkAsDeleted();
        await _unitOfWork.Repository<Company>().UpdateAsync(company, cancellationToken);
        await _unitOfWork.SaveAsync(cancellationToken);

        return true;
    }
}

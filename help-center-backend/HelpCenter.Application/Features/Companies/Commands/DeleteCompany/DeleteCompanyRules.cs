using HelpCenter.Application.Exceptions;
using HelpCenter.Application.Interfaces;
using HelpCenter.Domain.Entities;

namespace HelpCenter.Application.Features.Companies.Commands.DeleteCompany;

/// <summary>
/// Rules belonging only to the delete company slice.
/// </summary>
public class DeleteCompanyRules
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteCompanyRules(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Verifies the company exists in the system.
    /// </summary>
    public static void CompanyShouldExist(Company? company)
    {
        if (company == null || company.IsDeleted)
        {
            throw new CompanyNotFoundException();
        }
    }

    /// <summary>
    /// Verifies the company cannot be deleted while it has (any customer's) support
    /// requests attached to it.
    /// </summary>
    public async Task CompanyShouldNotHaveActiveRequestsAsync(int companyId, CancellationToken cancellationToken)
    {
        var hasRequests = await _unitOfWork.Repository<CustomerRequest>()
            .AnyAsync(r => r.Customer.CompanyId == companyId, cancellationToken: cancellationToken);

        if (hasRequests)
        {
            throw new CompanyHasActiveRequestsException();
        }
    }
}

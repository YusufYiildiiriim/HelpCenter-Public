using HelpCenter.Application.Exceptions;
using HelpCenter.Application.Interfaces;
using HelpCenter.Domain.Entities;

namespace HelpCenter.Application.Features.Companies.Commands.CreateCompany;

/// <summary>
/// Rules belonging only to the create company slice.
/// </summary>
public class CreateCompanyRules
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateCompanyRules(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Verifies the company name is unique.
    /// </summary>
    public async Task CompanyNameShouldBeUniqueAsync(string name, int? currentCompanyId, CancellationToken cancellationToken)
    {
        var existing = await _unitOfWork.Repository<Company>()
            .FindAsync(c => c.Name == name && (currentCompanyId == null || c.Id != currentCompanyId.Value), cancellationToken);

        if (existing.Any())
        {
            throw new CompanyNameAlreadyExistsException();
        }
    }

    /// <summary>
    /// Verifies the requested username has not been taken by someone else.
    /// </summary>
    public static void UsernameShouldBeAvailable(bool isTaken, string username)
    {
        if (isTaken)
        {
            throw new UsernameAlreadyExistsException($"'{username}' kullanıcı adı zaten kullanımda.");
        }
    }
}

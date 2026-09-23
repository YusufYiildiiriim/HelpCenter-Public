using HelpCenter.Application.Exceptions;
using HelpCenter.Application.Interfaces;
using HelpCenter.Domain.Entities;

namespace HelpCenter.Application.Features.Requests.Commands.CreateRequest;

/// <summary>
/// Rules belonging only to the request-creation slice.
/// </summary>
public class CreateRequestRules
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateRequestRules(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Verifies that the customer opening the request exists in the system.
    /// </summary>
    public static void CustomerShouldExist(Customer? customer)
    {
        if (customer == null)
        {
            throw new CustomerNotFoundException("Müşteri bulunamadı.");
        }
    }

    /// <summary>
    /// Checks that the selected module is defined for the customer's company or its associated project.
    /// </summary>
    public async Task ModuleShouldBeAllowedForCompanyAsync(int companyId, int moduleId, CancellationToken cancellationToken)
    {
        var module = await _unitOfWork.Repository<Module>()
            .FirstOrDefaultAsync(m => m.Id == moduleId && m.IsActive, cancellationToken: cancellationToken);

        if (module == null)
        {
            throw new ModuleNotAllowedForCompanyException();
        }

        var company = await _unitOfWork.Repository<Company>().GetAsync(companyId, cancellationToken);

        var companyModules = await _unitOfWork.Repository<CompanyModule>()
            .FindAsync(cm => cm.CompanyId == companyId, cancellationToken);

        var allowedModuleIds = companyModules.Select(cm => cm.ModuleId).ToList();

        if (company != null && company.ProjectId.HasValue)
        {
            var projectModules = await _unitOfWork.Repository<ProjectModule>()
                .FindAsync(pm => pm.ProjectId == company.ProjectId.Value, cancellationToken);

            allowedModuleIds = allowedModuleIds.Intersect(projectModules.Select(pm => pm.ModuleId)).ToList();
        }

        if (!allowedModuleIds.Contains(moduleId))
        {
            throw new ModuleNotAllowedForCompanyException();
        }
    }

    /// <summary>
    /// Locks the selected request subject if it is not already locked.
    /// </summary>
    public async Task LockSubjectIfUnlockedAsync(int? subjectId, CancellationToken cancellationToken)
    {
        if (subjectId.HasValue)
        {
            var subject = await _unitOfWork.Repository<RequestSubject>().GetAsync(subjectId.Value, cancellationToken);
            if (subject != null && !subject.IsLocked)
            {
                subject.Lock();
                await _unitOfWork.Repository<RequestSubject>().UpdateAsync(subject, cancellationToken);
            }
        }
    }

    /// <summary>
    /// Locks the selected module if it is not already locked.
    /// </summary>
    public async Task LockModuleIfUnlockedAsync(int moduleId, CancellationToken cancellationToken)
    {
        var module = await _unitOfWork.Repository<Module>().GetAsync(moduleId, cancellationToken);
        if (module == null)
        {
            throw new ModuleNotAllowedForCompanyException();
        }

        if (!module.IsLocked)
        {
            module.Lock();
            await _unitOfWork.Repository<Module>().UpdateAsync(module, cancellationToken);
        }
    }

    /// <summary>
    /// Locks a lockable entity (RequestSubject, Module) if it is not already locked. Both
    /// entities share the same IsLocked/Lock() pattern but have no common interface in Domain.
    /// </summary>
    private async Task LockIfUnlockedAsync<TEntity>(
        int? id,
        Func<TEntity, bool> isLocked,
        Action<TEntity> lockEntity,
        CancellationToken cancellationToken)
        where TEntity : class
    {
        if (!id.HasValue) return;

        var entity = await _unitOfWork.Repository<TEntity>().GetAsync(id.Value, cancellationToken);
        if (entity != null && !isLocked(entity))
        {
            lockEntity(entity);
            await _unitOfWork.Repository<TEntity>().UpdateAsync(entity, cancellationToken);
        }
    }
}

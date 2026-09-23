using System.Linq.Expressions;
using AutoMapper;
using HelpCenter.Application.Interfaces;
using HelpCenter.Domain.Entities;
using MediatR;

namespace HelpCenter.Application.Features.Companies.Commands.UpdateCompany;

public class UpdateCompanyCommandHandler : IRequestHandler<UpdateCompanyCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IPasswordService _passwordService;
    private readonly UpdateCompanyRules _rules;

    public UpdateCompanyCommandHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IPasswordService passwordService,
        UpdateCompanyRules rules)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _passwordService = passwordService;
        _rules = rules;
    }

    public async Task<bool> Handle(UpdateCompanyCommand request, CancellationToken cancellationToken)
    {
        var companyEntity = await _unitOfWork.Repository<Company>().FirstOrDefaultAsync(
            x => x.PublicId == request.PublicId,
            asTracking: true,
            cancellationToken: cancellationToken,
            x => x.CompanyModules);

        UpdateCompanyRules.CompanyShouldExist(companyEntity);

        await _rules.CompanyNameShouldBeUniqueAsync(request.Name, companyEntity!.Id, cancellationToken);

        if (request.RowVersion != null)
        {
            _unitOfWork.SetOriginalVersion(companyEntity!, request.RowVersion);
        }
        _mapper.Map(request, companyEntity);

        var moduleIds = request.ModuleIds;
        if (request.ProjectId.HasValue && moduleIds != null)
        {
            var projectModuleIds = (await _unitOfWork.Repository<ProjectModule>()
                .FindAsync(x => x.ProjectId == request.ProjectId.Value, cancellationToken))
                .Select(x => x.ModuleId);
            moduleIds = moduleIds.Intersect(projectModuleIds).ToList();
        }

        if (moduleIds != null)
        {
            companyEntity!.SyncModules(moduleIds);

            foreach (var moduleId in moduleIds)
            {
                var module = await _unitOfWork.Repository<Module>().GetAsync(moduleId, cancellationToken);
                if (module != null && !module.IsLocked)
                {
                    module.Lock();
                    await _unitOfWork.Repository<Module>().UpdateAsync(module, cancellationToken);
                }
            }
        }

        if (companyEntity != null)
        {
            await _unitOfWork.SaveAsync(cancellationToken);

            if (companyEntity.IsDemoActive)
            {
                var existingUser =
                    (await _unitOfWork.Repository<Customer>()
                        .FindAsync(x => x.Account.Email == companyEntity.ContactPersonEmail,
                            cancellationToken, x => x.Account)).FirstOrDefault();
                if (existingUser == null)
                {
                    var initPassword = !string.IsNullOrWhiteSpace(request.Password) ? request.Password : "123456";
                    var username = await ResolveUniqueUsernameAsync(
                        request.ContactPersonUsername,
                        companyEntity.ContactPersonName,
                        companyEntity.ContactPersonSurname,
                        cancellationToken);

                    var customer = Customer.Create(
                        companyEntity.Id,
                        companyEntity.ContactPersonName,
                        companyEntity.ContactPersonSurname,
                        companyEntity.ContactPersonEmail,
                        companyEntity.ContactPersonPhone,
                        _passwordService.HashPassword(initPassword),
                        username);
                    await _unitOfWork.Repository<Customer>().AddAsync(customer, cancellationToken);
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(request.ContactPersonUsername))
                    {
                        var normalized = Account.GenerateUsername(request.ContactPersonUsername, "").TrimEnd('.');
                        var isTaken = await _unitOfWork.Repository<Account>().AnyAsync(
                            x => x.Username == normalized && x.Id != existingUser.AccountId, cancellationToken: cancellationToken);
                        UpdateCompanyRules.UsernameShouldBeAvailable(isTaken, normalized);


                        existingUser.Account.Username = normalized;
                    }

                    await _unitOfWork.Repository<Customer>().UpdateAsync(existingUser, cancellationToken);
                }

                await _unitOfWork.SaveAsync(cancellationToken);
            }
        }

        return true;
    }

    private async Task<string> ResolveUniqueUsernameAsync(string? requested, string firstName, string lastName, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(requested))
        {
            var normalized = Account.GenerateUsername(requested, "").TrimEnd('.');
            var isTaken = await _unitOfWork.Repository<Account>().AnyAsync(x => x.Username == normalized, cancellationToken: cancellationToken);
            UpdateCompanyRules.UsernameShouldBeAvailable(isTaken, normalized);
            return normalized;
        }

        var baseUsername = Account.GenerateUsername(firstName, lastName);
        var username = baseUsername;
        var counter = 1;
        while (await _unitOfWork.Repository<Account>().AnyAsync(x => x.Username == username, cancellationToken: cancellationToken))
        {
            username = $"{baseUsername}{counter}";
            counter++;
        }
        return username;
    }
}

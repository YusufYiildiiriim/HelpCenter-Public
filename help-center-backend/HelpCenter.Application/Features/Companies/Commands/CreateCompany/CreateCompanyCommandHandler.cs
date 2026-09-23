using AutoMapper;
using HelpCenter.Application.Interfaces;
using HelpCenter.Domain.Entities;
using MediatR;

namespace HelpCenter.Application.Features.Companies.Commands.CreateCompany;

public class CreateCompanyCommandHandler : IRequestHandler<CreateCompanyCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IPasswordService _passwordService;
    private readonly CreateCompanyRules _rules;

    public CreateCompanyCommandHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IPasswordService passwordService,
        CreateCompanyRules rules)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _passwordService = passwordService;
        _rules = rules;
    }

    public async Task<bool> Handle(CreateCompanyCommand request, CancellationToken cancellationToken)
    {
        await _rules.CompanyNameShouldBeUniqueAsync(request.Name, null, cancellationToken);

        var company = _mapper.Map<Company>(request);

        var moduleIds = request.ModuleIds;
        if (request.ProjectId.HasValue && moduleIds != null && moduleIds.Any())
        {
            var projectModuleIds = (await _unitOfWork.Repository<ProjectModule>()
                .FindAsync(x => x.ProjectId == request.ProjectId.Value, cancellationToken))
                .Select(x => x.ModuleId);
            moduleIds = moduleIds.Intersect(projectModuleIds).ToList();
        }

        if (moduleIds != null && moduleIds.Any())
        {
            company.SyncModules(moduleIds);

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

        await _unitOfWork.Repository<Company>().AddAsync(company, cancellationToken);
        await _unitOfWork.SaveAsync(cancellationToken);

        if (request.IsDemoActive && !string.IsNullOrEmpty(request.Password))
        {
            var username = await ResolveUniqueUsernameAsync(
                request.ContactPersonUsername,
                request.ContactPersonName,
                request.ContactPersonSurname,
                cancellationToken);

            var customer = Customer.Create(
                company.Id,
                request.ContactPersonName,
                request.ContactPersonSurname,
                request.ContactPersonEmail,
                request.ContactPersonPhone,
                _passwordService.HashPassword(request.Password),
                username);

            await _unitOfWork.Repository<Customer>().AddAsync(customer, cancellationToken);
            await _unitOfWork.SaveAsync(cancellationToken);
        }

        return true;
    }

    private async Task<string> ResolveUniqueUsernameAsync(string? requested, string firstName, string lastName, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(requested))
        {
            var normalized = Account.GenerateUsername(requested, "").TrimEnd('.');
            var isTaken = await _unitOfWork.Repository<Account>().AnyAsync(x => x.Username == normalized, cancellationToken: cancellationToken);
            CreateCompanyRules.UsernameShouldBeAvailable(isTaken, normalized);
            return normalized;
        }

        var baseUsername = Account.GenerateUsername(firstName, lastName);
        var candidate = baseUsername;
        var suffix = 1;
        while (await _unitOfWork.Repository<Account>().AnyAsync(x => x.Username == candidate, cancellationToken: cancellationToken))
        {
            candidate = $"{baseUsername}{suffix}";
            suffix++;
        }
        return candidate;
    }
}

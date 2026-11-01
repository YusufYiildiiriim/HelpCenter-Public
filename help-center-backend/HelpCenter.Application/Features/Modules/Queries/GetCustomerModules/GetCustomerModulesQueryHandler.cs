using AutoMapper;
using HelpCenter.Application.Interfaces;
using HelpCenter.Domain.Entities;
using MediatR;

namespace HelpCenter.Application.Features.Modules.Queries.GetCustomerModules;

public class GetCustomerModulesQueryHandler : IRequestHandler<GetCustomerModulesQuery, List<CustomerModuleDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetCustomerModulesQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<List<CustomerModuleDto>> Handle(GetCustomerModulesQuery request, CancellationToken cancellationToken)
    {
        Guid targetCompanyId = request.CompanyId;
        Guid? targetProjectId = null;
        string? companyLocale = null;

        if (targetCompanyId == Guid.Empty && request.CustomerId != Guid.Empty)
        {
            var customer = (await _unitOfWork.Repository<Customer>()
                .FindAsync(x => x.Id == request.CustomerId, cancellationToken, c => c.Company))
                .FirstOrDefault();

            if (customer != null && customer.Company != null)
            {
                targetCompanyId = customer.CompanyId;
                targetProjectId = customer.Company.ProjectId;
                companyLocale = customer.Company.DefaultLocale;
            }
        }
        else if (targetCompanyId != Guid.Empty)
        {
            var company = await _unitOfWork.Repository<Company>()
                .GetAsync(targetCompanyId, cancellationToken);
            targetProjectId = company?.ProjectId;
            companyLocale = company?.DefaultLocale;
        }

        if (targetCompanyId == Guid.Empty)
        {
            return new List<CustomerModuleDto>();
        }

        var companyModules = await _unitOfWork.Repository<CompanyModule>()
            .FindAsync(cm => cm.CompanyId == targetCompanyId, cancellationToken);

        var allowedModuleIds = companyModules.Select(cm => cm.ModuleId).ToList();

        if (targetProjectId.HasValue)
        {
            var projectModules = await _unitOfWork.Repository<ProjectModule>()
                .FindAsync(pm => pm.ProjectId == targetProjectId.Value, cancellationToken);

            var projectModuleIds = projectModules.Select(pm => pm.ModuleId).ToList();
            allowedModuleIds = allowedModuleIds.Intersect(projectModuleIds).ToList();
        }



        if (!allowedModuleIds.Any())
        {
            return new List<CustomerModuleDto>();
        }

        var modules = await _unitOfWork.Repository<Module>()
            .FindAsync(x => allowedModuleIds.Contains(x.Id) && x.IsActive, cancellationToken, x => x.Translations);

        var requestedLocale = HelpCenter.Domain.Common.SupportedLocales.Normalize(request.Locale)
            ?? HelpCenter.Domain.Common.SupportedLocales.Default;
        var fallbackLocale = HelpCenter.Domain.Common.SupportedLocales.Normalize(companyLocale)
            ?? HelpCenter.Domain.Common.SupportedLocales.Default;
        return modules.Select(module =>
        {
            var translation = module.Translations.FirstOrDefault(x => x.Locale == requestedLocale)
                ?? module.Translations.FirstOrDefault(x => x.Locale == fallbackLocale)
                ?? (module.SourceLocale is null ? null : module.Translations.FirstOrDefault(x => x.Locale == module.SourceLocale));
            return new CustomerModuleDto
            {
                Id = module.Id,
                Name = translation?.Name ?? module.Name,
                RequestedLocale = requestedLocale,
                ContentLocale = translation?.Locale ?? module.SourceLocale
            };
        }).ToList();
    }
}

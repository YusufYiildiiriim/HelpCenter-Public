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
        int targetCompanyId = request.CompanyId;
        int? targetProjectId = null;

        if (targetCompanyId == 0 && request.CustomerId > 0)
        {
            var customer = (await _unitOfWork.Repository<Customer>()
                .FindAsync(x => x.Id == request.CustomerId, cancellationToken, c => c.Company))
                .FirstOrDefault();

            if (customer != null && customer.Company != null)
            {
                targetCompanyId = customer.CompanyId;
                targetProjectId = customer.Company.ProjectId;
            }
        }
        else if (targetCompanyId > 0)
        {
            var company = await _unitOfWork.Repository<Company>()
                .GetAsync(targetCompanyId, cancellationToken);
            targetProjectId = company?.ProjectId;
        }

        if (targetCompanyId == 0)
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
            .FindAsync(x => allowedModuleIds.Contains(x.Id) && x.IsActive, cancellationToken);

        return _mapper.Map<List<CustomerModuleDto>>(modules);
    }
}

using AutoMapper;
using HelpCenter.Application.Interfaces;
using HelpCenter.Domain.Entities;
using MediatR;

namespace HelpCenter.Application.Features.Faqs.Queries.GetCustomerFaqs;

public class GetCustomerFaqsQueryHandler : IRequestHandler<GetCustomerFaqsQuery, List<CustomerFaqDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetCustomerFaqsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<List<CustomerFaqDto>> Handle(GetCustomerFaqsQuery request, CancellationToken cancellationToken)
    {
        var faqs = await _unitOfWork.Repository<FAQ>().FindAsync(
            x => x.IsActive
                 && (!request.ProjectId.HasValue || x.ProjectId == request.ProjectId.Value)
                 && (!request.ModuleId.HasValue || x.ModuleId == request.ModuleId.Value),
            cancellationToken,
            x => x.Project!,
            x => x.Module!
        );

        return _mapper.Map<List<CustomerFaqDto>>(faqs);
    }
}

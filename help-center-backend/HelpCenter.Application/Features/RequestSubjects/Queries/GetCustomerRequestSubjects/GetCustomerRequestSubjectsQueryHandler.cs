using AutoMapper;
using HelpCenter.Application.Interfaces;
using HelpCenter.Domain.Entities;
using MediatR;

namespace HelpCenter.Application.Features.RequestSubjects.Queries.GetCustomerRequestSubjects;

public class GetCustomerRequestSubjectsQueryHandler : IRequestHandler<GetCustomerRequestSubjectsQuery, List<CustomerRequestSubjectDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetCustomerRequestSubjectsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<List<CustomerRequestSubjectDto>> Handle(GetCustomerRequestSubjectsQuery request, CancellationToken cancellationToken)
    {
        var subjects = await _unitOfWork.Repository<RequestSubject>().FindAsync(
            x => x.IsActive,
            cancellationToken
        );

        return _mapper.Map<List<CustomerRequestSubjectDto>>(subjects);
    }
}

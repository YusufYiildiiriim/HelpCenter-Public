using AutoMapper;
using HelpCenter.Application.Interfaces;
using HelpCenter.Domain.Entities;
using MediatR;

namespace HelpCenter.Application.Features.RequestSubjects.Queries.GetPublicRequestSubjects;

public class GetPublicRequestSubjectsQueryHandler : IRequestHandler<GetPublicRequestSubjectsQuery, List<PublicRequestSubjectDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetPublicRequestSubjectsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<List<PublicRequestSubjectDto>> Handle(GetPublicRequestSubjectsQuery request, CancellationToken cancellationToken)
    {
        var subjects = await _unitOfWork.Repository<RequestSubject>().FindAsync(x => x.IsActive, cancellationToken);
        return _mapper.Map<List<PublicRequestSubjectDto>>(subjects);
    }
}

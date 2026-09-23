using AutoMapper;
using HelpCenter.Application.Interfaces;
using HelpCenter.Domain.Entities;
using MediatR;

namespace HelpCenter.Application.Features.Modules.Queries.GetExpertsByModuleId;

public class GetExpertsByModuleIdQueryHandler : IRequestHandler<GetExpertsByModuleIdQuery, List<ModuleExpertDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetExpertsByModuleIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<List<ModuleExpertDto>> Handle(GetExpertsByModuleIdQuery request, CancellationToken cancellationToken)
    {
        var experts = await _unitOfWork.Repository<ModuleExpert>()
            .FindAsync(x => x.ModuleId == request.ModuleId, cancellationToken, x => x.User, x => x.User.Account);

        return _mapper.Map<List<ModuleExpertDto>>(experts);
    }
}

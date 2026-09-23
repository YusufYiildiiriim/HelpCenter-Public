using AutoMapper;
using HelpCenter.Application.Interfaces;
using HelpCenter.Domain.Entities;
using MediatR;

namespace HelpCenter.Application.Features.Projects.Queries.GetUsersByProjectId;

public class GetUsersByProjectIdQueryHandler : IRequestHandler<GetUsersByProjectIdQuery, List<ProjectUserDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetUsersByProjectIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<List<ProjectUserDto>> Handle(GetUsersByProjectIdQuery request, CancellationToken cancellationToken)
    {
        var userProjects = await _unitOfWork.Repository<UserProject>()
            .FindAsync(x => x.ProjectId == request.ProjectId, cancellationToken, x => x.User, x => x.User.Account);

        return _mapper.Map<List<ProjectUserDto>>(userProjects);
    }
}

using AutoMapper;
using HelpCenter.Application.Features.Guides.Queries.GetPublicGuides;
using HelpCenter.Application.Interfaces;
using HelpCenter.Domain.Entities;
using MediatR;

namespace HelpCenter.Application.Features.Guides.Queries.GetCustomerGuides;

public class GetCustomerGuidesQueryHandler : IRequestHandler<GetCustomerGuidesQuery, List<PublicGuideDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetCustomerGuidesQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<List<PublicGuideDto>> Handle(GetCustomerGuidesQuery request, CancellationToken cancellationToken)
    {
        var guides = (await _unitOfWork.Repository<Guide>().FindAsync(
            g => g.IsActive,
            cancellationToken,
            g => g.Documents!
        )).ToList();

        var dtos = _mapper.Map<List<PublicGuideDto>>(guides);

        var previousGuideIds = guides.Where(g => g.PreviousGuideId.HasValue)
            .Select(g => g.PreviousGuideId!.Value).Distinct().ToList();
        var previousGuidePublicIds = previousGuideIds.Count > 0
            ? (await _unitOfWork.Repository<Guide>().SelectAsync(
                g => previousGuideIds.Contains(g.Id),
                g => new { g.Id, g.PublicId },
                cancellationToken: cancellationToken))
              .ToDictionary(x => x.Id, x => x.PublicId)
            : new Dictionary<int, Guid>();

        for (int i = 0; i < guides.Count; i++)
        {
            dtos[i].PublicId = guides[i].PublicId;
            dtos[i].PreviousGuidePublicId = guides[i].PreviousGuideId is int prevId
                && previousGuidePublicIds.TryGetValue(prevId, out var prevPublicId)
                ? prevPublicId
                : null;
        }

        return dtos;
    }
}

using AutoMapper;
using HelpCenter.Application.Common.Caching;
using HelpCenter.Application.Interfaces;
using HelpCenter.Domain.Entities;
using MediatR;

namespace HelpCenter.Application.Features.Guides.Commands.CreateGuide;

public class CreateGuideCommandHandler : IRequestHandler<CreateGuideCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IFileService _fileService;
    private readonly ICacheService _cache;
    private readonly CreateGuideRules _rules;

    public CreateGuideCommandHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IFileService fileService,
        ICacheService cache,
        CreateGuideRules rules)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _fileService = fileService;
        _cache = cache;
        _rules = rules;
    }

    public async Task<bool> Handle(CreateGuideCommand request, CancellationToken cancellationToken)
    {
        await _rules.GuideTitleShouldBeUniqueAsync(request.Title, null, cancellationToken);

        var entity = _mapper.Map<Guide>(request);
        entity.IsActive = request.IsActive;
        entity.IsPublic = request.IsPublic;

        if (request.Files != null && request.Files.Count > 0)
        {
            foreach (var file in request.Files)
            {
                string webPath = await _fileService.SaveFileAsync(file, "guides");
                entity.AddDocument(webPath, file.FileName);
            }
        }

        await _unitOfWork.Repository<Guide>().AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveAsync(cancellationToken);

        _cache.InvalidatePrefix(CacheKeys.PublicGuidesPrefix);
        _cache.InvalidatePrefix(CacheKeys.PublicModulesPrefix);
        return true;
    }
}

using AutoMapper;
using HelpCenter.Application.Common.Caching;
using HelpCenter.Application.Interfaces;
using HelpCenter.Domain.Entities;
using MediatR;

namespace HelpCenter.Application.Features.Guides.Commands.UpdateGuide;

public class UpdateGuideCommandHandler : IRequestHandler<UpdateGuideCommand, bool>
{
    private readonly IFileService _fileService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;
    private readonly UpdateGuideRules _rules;

    public UpdateGuideCommandHandler(
        IUnitOfWork unitOfWork,
        IFileService fileService,
        ICacheService cache,
        UpdateGuideRules rules)
    {
        _unitOfWork = unitOfWork;
        _fileService = fileService;
        _cache = cache;
        _rules = rules;
    }

    public async Task<bool> Handle(UpdateGuideCommand request, CancellationToken cancellationToken)
    {
        var guideRepository = _unitOfWork.Repository<Guide>();
        var entity = (await guideRepository.FindAsync(x => x.Id == request.Id, cancellationToken, x => x.Documents!)).FirstOrDefault();
        UpdateGuideRules.GuideShouldExist(entity);

        await _rules.GuideTitleShouldBeUniqueAsync(request.Title, entity!.Id, cancellationToken);

        if (request.RowVersion != null)
            _unitOfWork.SetOriginalVersion(entity!, request.RowVersion);

        entity!.UpdateInfo(
            request.Title,
            request.Description,
            request.Module,
            request.YoutubeUrl,
            request.PreviousGuideId,
            request.IsActive,
            request.IsPublic
        );

        var module = await HelpCenter.Application.Features.Guides.GuideModuleResolver.ResolveAsync(_unitOfWork, request.ModuleId, request.Module, cancellationToken);
        entity!.ModuleId = module.Id;
        entity.Module = module.Name;

        if (request.Files != null && request.Files.Count > 0)
        {
            entity.ClearDocuments();

            foreach (var file in request.Files)
            {
                string webPath = await _fileService.SaveFileAsync(file, "guides");
                entity.AddDocument(webPath, file.FileName);
            }
        }

        await guideRepository.UpdateAsync(entity);
        var result = await _unitOfWork.SaveAsync(cancellationToken);

        _cache.Invalidate(CacheCategory.PublicGuides);
        _cache.Invalidate(CacheCategory.PublicModules);
        return result > 0;
    }
}

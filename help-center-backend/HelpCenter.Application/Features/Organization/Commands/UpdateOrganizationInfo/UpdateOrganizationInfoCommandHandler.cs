using HelpCenter.Application.Common.Caching;
using HelpCenter.Application.Interfaces;
using HelpCenter.Domain.Entities;
using MediatR;

namespace HelpCenter.Application.Features.Organization.Commands.UpdateOrganizationInfo;

public class UpdateOrganizationInfoCommandHandler : IRequestHandler<UpdateOrganizationInfoCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;
    private readonly IAuditLogWriter _auditLog;
    private readonly IFileService _fileService;

    public UpdateOrganizationInfoCommandHandler(
        IUnitOfWork unitOfWork,
        ICacheService cache,
        IAuditLogWriter auditLog,
        IFileService fileService)
    {
        _unitOfWork = unitOfWork;
        _cache = cache;
        _auditLog = auditLog;
        _fileService = fileService;
    }

    public async Task<bool> Handle(UpdateOrganizationInfoCommand request, CancellationToken cancellationToken)
    {
        var org = await _unitOfWork.Repository<OrganizationInfo>().FirstOrDefaultAsync(
            x => x.Id == request.Id,
            cancellationToken: cancellationToken);

        UpdateOrganizationInfoRules.OrganizationInfoShouldExist(org);

        var before = new
        {
            org!.OrganizationName,
            org.Phone,
            org.Email,
            org.Address,
            org.Website,
            org.TaxNumber,
            org.TaxOffice,
            org.FooterText,
            org.LogoUrl
        };

        if (request.RowVersion != null)
            _unitOfWork.SetOriginalVersion(org!, request.RowVersion);

        org.Update(
            request.OrganizationName,
            request.Phone,
            request.Email,
            request.Address,
            request.Website,
            request.TaxNumber,
            request.TaxOffice,
            request.FooterText);

        if (request.LogoUrl != null)
            org.SetLogo(request.LogoUrl);

        await _unitOfWork.Repository<OrganizationInfo>().UpdateAsync(org, cancellationToken);
        await _unitOfWork.SaveAsync(cancellationToken);

        // Delete the old logo file ONLY AFTER the DB update (including the RowVersion check)
        // has successfully COMMITTED. If SaveAsync fails due to a concurrency conflict or any
        // other DB error, this line is never reached — the old file stays on disk and the
        // organization still points to a valid logo.
        if (request.LogoUrl != null && !string.IsNullOrWhiteSpace(before.LogoUrl))
            _fileService.DeleteFile(before.LogoUrl);

        var after = new
        {
            org.OrganizationName,
            org.Phone,
            org.Email,
            org.Address,
            org.Website,
            org.TaxNumber,
            org.TaxOffice,
            org.FooterText,
            org.LogoUrl
        };

        await _auditLog.WriteDiffAsync("OrganizationInfoUpdated", "OrganizationInfo", org.Id, before, after, cancellationToken);

        _cache.InvalidatePrefix(CacheKeys.OrganizationInfoPrefix);
        return true;
    }
}

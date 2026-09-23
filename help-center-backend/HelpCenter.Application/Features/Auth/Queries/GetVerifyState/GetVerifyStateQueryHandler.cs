using HelpCenter.Application.Features.Roles.Queries.GetUserPermissions;
using HelpCenter.Application.Interfaces;
using HelpCenter.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HelpCenter.Application.Features.Auth.Queries.GetVerifyState;

public class GetVerifyStateQueryHandler : IRequestHandler<GetVerifyStateQuery, GetVerifyStateResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserPermissionsReader _permissionsReader;
    private readonly ILogger<GetVerifyStateQueryHandler> _logger;

    public GetVerifyStateQueryHandler(
        IUnitOfWork unitOfWork,
        UserPermissionsReader permissionsReader,
        ILogger<GetVerifyStateQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _permissionsReader = permissionsReader;
        _logger = logger;
    }

    public async Task<GetVerifyStateResponse> Handle(GetVerifyStateQuery request, CancellationToken cancellationToken)
    {
        var result = new GetVerifyStateResponse();

        if (request.UserId <= 0) return result;

        // The verify flow is "best effort": a single failed read must not drop the
        // session verification entirely. The error is still logged.
        try
        {
            result.Permissions = await _permissionsReader.ReadAsync(request.UserId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Verify state: kullanıcı izinleri okunamadı. UserId: {UserId}", request.UserId);
        }

        try
        {
            // Instead of branching on role name, we look at which physical table the
            // identity lives in: internal staff (Admin/Agent/Expert/any future role)
            // lives in the User table, while Customer/Company identities live in the
            // Customer table. That way, creating a new role (e.g. "Manager") from the
            // admin panel requires no code change here.
            var users = await _unitOfWork.Repository<User>().FindAsync(u => u.Id == request.UserId, cancellationToken, u => u.Account);
            var user = users.FirstOrDefault();
            if (user != null)
            {
                result.IsPasswordChangeRequired = user.IsPasswordChangeRequired;
            }
            else
            {
                var customers = await _unitOfWork.Repository<Customer>().FindAsync(c => c.Id == request.UserId, cancellationToken, c => c.Account);
                result.IsPasswordChangeRequired = customers.FirstOrDefault()?.IsPasswordChangeRequired ?? false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Verify state: şifre değişim durumu okunamadı. UserId: {UserId}", request.UserId);
        }

        return result;
    }
}

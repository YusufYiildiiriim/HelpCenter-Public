using MediatR;

namespace HelpCenter.Application.Features.Roles.Queries.GetUserPermissions;

public class GetUserPermissionsQueryHandler : IRequestHandler<GetUserPermissionsQuery, UserPermissionsDto>
{
    private readonly UserPermissionsReader _reader;

    public GetUserPermissionsQueryHandler(UserPermissionsReader reader)
    {
        _reader = reader;
    }

    public Task<UserPermissionsDto> Handle(GetUserPermissionsQuery request, CancellationToken cancellationToken) =>
        _reader.ReadAsync(request.UserId, cancellationToken);
}

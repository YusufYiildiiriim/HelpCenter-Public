using MediatR;

namespace HelpCenter.Application.Features.Requests.Queries.GetAdminMessages;

public class GetAdminMessagesQuery : IRequest<AdminMessagesResponse>
{
    public Guid RequestPublicId { get; set; }
    public int PageSize { get; set; } = 20;
    public int? BeforeId { get; set; }
    public bool OnlyAssignedToCurrentUser { get; set; }
}

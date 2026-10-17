using HelpCenter.Application.Common.Caching;
using HelpCenter.Domain.Enums;
using MediatR;
using HelpCenter.Application.Common.Models;

namespace HelpCenter.Application.Features.Requests.Commands.CreateRequest;

public class CreateRequestCommand : IRequest<CreateRequestResponse>, IInvalidatesMessageCache
{
    public string Title { get; set; } = string.Empty;
    public string MessageText { get; set; } = string.Empty;
    public Guid CustomerId { get; set; }
    public Guid? ModuleId { get; set; }
    public Guid? RequestSubjectId { get; set; }
    public RequestPriority Priority { get; set; } = RequestPriority.Medium;
    public List<FileUpload>? Files { get; set; }
}

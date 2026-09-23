using HelpCenter.Domain.Enums;
using MediatR;
using HelpCenter.Application.Common.Models;

namespace HelpCenter.Application.Features.Requests.Commands.CreateRequest;

public class CreateRequestCommand : IRequest<CreateRequestResponse>
{
    public string Title { get; set; } = string.Empty;
    public string MessageText { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public int? ModuleId { get; set; }
    public int? RequestSubjectId { get; set; }
    public RequestPriority Priority { get; set; } = RequestPriority.Medium;
    public List<FileUpload>? Files { get; set; }
}

using MediatR;

namespace HelpCenter.Application.Features.Customers.Commands.UpdateProfile;

public record UpdateProfileCommand : IRequest<bool>
{
    public Guid CustomerId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
}

using MediatR;

namespace HelpCenter.Application.Features.Customers.Commands.UpdateCustomer;

public class UpdateCustomerCommand : IRequest<bool>
{
    public Guid PublicId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public int CompanyId { get; set; }
    public bool IsActive { get; set; }
    public string? Password { get; set; }
    public string? Username { get; set; }
}

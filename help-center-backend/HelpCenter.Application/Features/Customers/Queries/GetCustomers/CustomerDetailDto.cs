namespace HelpCenter.Application.Features.Customers.Queries.GetCustomers;

public class CustomerDetailDto
{
    public int Id { get; set; }
    public Guid PublicId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public int CompanyId { get; set; }
    public bool IsActive { get; set; }
}

using HelpCenter.Application.Interfaces;
using HelpCenter.Domain.Entities;
using MediatR;

namespace HelpCenter.Application.Features.Customers.Commands.CreateCustomer;

public class CreateCustomerCommandHandler : IRequestHandler<CreateCustomerCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordService _passwordService;
    private readonly IAuditLogWriter _auditLog;

    public CreateCustomerCommandHandler(IUnitOfWork unitOfWork, IPasswordService passwordService, IAuditLogWriter auditLog)
    {
        _unitOfWork = unitOfWork;
        _passwordService = passwordService;
        _auditLog = auditLog;
    }

    public async Task<bool> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
    {
        var existing = (await _unitOfWork.Repository<Customer>().FindAsync(x => x.Account.Email == request.Email, cancellationToken, x => x.Account)).FirstOrDefault();
        if (existing != null) return false;

        var username = await ResolveUniqueUsernameAsync(request.Username, request.FirstName, request.LastName, cancellationToken);

        var customer = Customer.Create(request.CompanyId, request.FirstName, request.LastName, request.Email, request.PhoneNumber, _passwordService.HashPassword(request.Password), username);
        customer.IsActive = request.IsActive;

        await _unitOfWork.Repository<Customer>().AddAsync(customer, cancellationToken);
        // AuditLogWriter shares this request's EfContext. Its SaveChanges persists this new
        // customer and the audit row in the same EF Core transaction.
        await _auditLog.WriteAsync("CustomerCreated", "Customer", targetId: null,
            new { TargetPublicId = customer.PublicId, customer.CompanyId, customer.Account.Email, Username = username }, cancellationToken);
        await _unitOfWork.SaveAsync(cancellationToken);

        return true;
    }

    private async Task<string> ResolveUniqueUsernameAsync(string? requested, string firstName, string lastName, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(requested))
        {
            var normalized = Account.GenerateUsername(requested, "").TrimEnd('.');
            var isTaken = await _unitOfWork.Repository<Account>().AnyAsync(x => x.Username == normalized, cancellationToken: cancellationToken);
            CreateCustomerRules.UsernameShouldBeAvailable(isTaken, normalized);
            return normalized;
        }

        var baseUsername = Account.GenerateUsername(firstName, lastName);
        var candidate = baseUsername;
        var suffix = 1;

        while (await _unitOfWork.Repository<Account>().AnyAsync(x => x.Username == candidate, cancellationToken: cancellationToken))
        {
            candidate = $"{baseUsername}{suffix}";
            suffix++;
        }

        return candidate;
    }
}

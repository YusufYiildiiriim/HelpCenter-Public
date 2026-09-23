using HelpCenter.Application.Interfaces;
using HelpCenter.Domain.Entities;
using MediatR;

namespace HelpCenter.Application.Features.Customers.Commands.UpdateCustomer;

public class UpdateCustomerCommandHandler : IRequestHandler<UpdateCustomerCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditLogWriter _auditLog;

    public UpdateCustomerCommandHandler(IUnitOfWork unitOfWork, IAuditLogWriter auditLog)
    {
        _unitOfWork = unitOfWork;
        _auditLog = auditLog;
    }

    public async Task<bool> Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = (await _unitOfWork.Repository<Customer>().FindAsync(x => x.PublicId == request.PublicId, cancellationToken, x => x.Account)).FirstOrDefault();
        UpdateCustomerRules.CustomerShouldExist(customer);

        var before = new
        {
            customer!.PublicId,
            customer.CompanyId,
            customer.FirstName,
            customer.LastName,
            customer.Email,
            customer.PhoneNumber,
            Username = customer.Account.Username,
            customer.IsActive
        };

        customer.FirstName = request.FirstName;
        customer.LastName = request.LastName;
        customer.Email = request.Email;
        customer.PhoneNumber = request.PhoneNumber;
        customer.CompanyId = request.CompanyId;
        customer.IsActive = request.IsActive;

        if (!string.IsNullOrWhiteSpace(request.Username))
        {
            var normalized = Account.GenerateUsername(request.Username, "").TrimEnd('.');
            var isTaken = await _unitOfWork.Repository<Account>().AnyAsync(
                x => x.Username == normalized && x.Id != customer.AccountId, cancellationToken: cancellationToken);
            UpdateCustomerRules.UsernameShouldBeAvailable(isTaken, normalized);
            customer.Account.Username = normalized;
        }

        customer.Account.UpdatedAt = DateTime.UtcNow;
        customer.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Repository<Account>().UpdateAsync(customer.Account, cancellationToken);
        await _unitOfWork.Repository<Customer>().UpdateAsync(customer, cancellationToken);
        var after = new
        {
            customer.PublicId,
            customer.CompanyId,
            customer.FirstName,
            customer.LastName,
            customer.Email,
            customer.PhoneNumber,
            Username = customer.Account.Username,
            customer.IsActive
        };

        await _auditLog.WriteDiffAsync("CustomerUpdated", "Customer", customer.Id, before, after, cancellationToken);
        await _unitOfWork.SaveAsync(cancellationToken);

        return true;
    }
}

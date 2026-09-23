using HelpCenter.Application.Interfaces;
using HelpCenter.Domain.Entities;
using MediatR;

namespace HelpCenter.Application.Features.Customers.Commands.DeleteCustomer;

public class DeleteCustomerCommandHandler : IRequestHandler<DeleteCustomerCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditLogWriter _auditLog;

    public DeleteCustomerCommandHandler(IUnitOfWork unitOfWork, IAuditLogWriter auditLog)
    {
        _unitOfWork = unitOfWork;
        _auditLog = auditLog;
    }

    public async Task<bool> Handle(DeleteCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = (await _unitOfWork.Repository<Customer>().FindAsync(x => x.PublicId == request.PublicId, cancellationToken)).FirstOrDefault();
        DeleteCustomerRules.CustomerShouldExist(customer);

        var before = new
        {
            customer!.PublicId,
            customer.CompanyId,
            customer.FirstName,
            customer.LastName,
            customer.Email,
            customer.PhoneNumber,
            customer.IsActive,
            customer.IsDeleted
        };

        customer.IsDeleted = true;
        customer.UpdatedAt = DateTime.Now;

        await _unitOfWork.Repository<Customer>().UpdateAsync(customer, cancellationToken);
        var after = new
        {
            customer.PublicId,
            customer.CompanyId,
            customer.FirstName,
            customer.LastName,
            customer.Email,
            customer.PhoneNumber,
            customer.IsActive,
            customer.IsDeleted
        };

        await _auditLog.WriteDiffAsync("CustomerDeleted", "Customer", customer.Id, before, after, cancellationToken);
        await _unitOfWork.SaveAsync(cancellationToken);

        return true;
    }
}

using System.Linq.Expressions;
using HelpCenter.Application.Interfaces;
using HelpCenter.Domain.Entities;
using MediatR;

namespace HelpCenter.Application.Features.Customers.Commands.UpdateProfile;

public class UpdateProfileCommandHandler : IRequestHandler<UpdateProfileCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateProfileCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
    {
        var customer = await _unitOfWork.Repository<Customer>().FirstOrDefaultAsync(
            x => x.Id == request.CustomerId && !x.IsDeleted,
            asTracking: false,
            cancellationToken: cancellationToken,
            x => x.Account);

        UpdateProfileRules.CustomerShouldExist(customer);

        customer!.Account.UpdateInfo(request.FirstName, request.LastName, phoneNumber: request.PhoneNumber);
        customer.Account.UpdatedAt = DateTime.UtcNow;
        customer.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Repository<Account>().UpdateAsync(customer.Account, cancellationToken);
        await _unitOfWork.Repository<Customer>().UpdateAsync(customer, cancellationToken);
        return await _unitOfWork.SaveAsync(cancellationToken) > 0;
    }
}

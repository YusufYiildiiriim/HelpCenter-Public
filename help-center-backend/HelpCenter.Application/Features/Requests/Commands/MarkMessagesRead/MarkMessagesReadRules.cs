using HelpCenter.Application.Exceptions;
using HelpCenter.Domain.Entities;

namespace HelpCenter.Application.Features.Requests.Commands.MarkMessagesRead;

public static class MarkMessagesReadRules
{
    public static void ReaderShouldBeAuthenticated(bool authenticated, int userId)
    {
        if (!authenticated || userId <= 0)
            throw new UnauthorizedException("Kimlik doğrulaması gerekli.");
    }

    public static void ReaderShouldMatchPortal(bool isCustomer, bool isAgent)
    {
        if (isCustomer == isAgent)
            throw new ForbiddenException("Bu işlem için yetkiniz yok.");
    }

    public static void RequestShouldExist(CustomerRequest? request)
    {
        if (request == null || request.IsDeleted) throw new RequestNotFoundException();
    }

    public static void RequestShouldBelongToCustomer(CustomerRequest request, int customerId)
    {
        if (request.CustomerId != customerId) throw new RequestNotBelongToCustomerException();
    }

    public static void StaffShouldHaveReadAccess(bool allowed)
    {
        if (!allowed) throw new ForbiddenException("Bu talebe erişim yetkiniz yok.");
    }
}

using HelpCenter.Application.Exceptions;

namespace HelpCenter.Application.Features.Customers.Commands.CreateCustomer;

/// <summary>
/// Rules belonging only to the create customer slice.
/// </summary>
public static class CreateCustomerRules
{
    /// <summary>
    /// Verifies the requested username has not been taken by someone else.
    /// </summary>
    public static void UsernameShouldBeAvailable(bool isTaken, string username)
    {
        if (isTaken)
        {
            throw new UsernameAlreadyExistsException($"'{username}' kullanıcı adı zaten kullanımda.");
        }
    }
}

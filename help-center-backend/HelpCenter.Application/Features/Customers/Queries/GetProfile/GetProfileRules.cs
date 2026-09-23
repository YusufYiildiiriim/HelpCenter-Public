using HelpCenter.Application.Exceptions;
using HelpCenter.Domain.Entities;

namespace HelpCenter.Application.Features.Customers.Queries.GetProfile;

/// <summary>
/// Rules belonging only to the view customer profile slice.
/// </summary>
public static class GetProfileRules
{
    /// <summary>
    /// Verifies the profile exists.
    /// </summary>
    public static void ProfileShouldExist(Customer? customer)
    {
        if (customer == null)
        {
            throw new CustomerNotFoundException("Profil bulunamadı.");
        }
    }
}

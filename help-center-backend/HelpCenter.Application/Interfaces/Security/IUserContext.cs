namespace HelpCenter.Application.Interfaces;

/// <summary>
/// Identity of the user making the current request, plus request metadata. The implementation
/// lives in the presentation/infrastructure layer (HTTP-based, or system-based for background jobs);
/// the Application layer does not know about HttpContext directly.
/// </summary>
public interface IUserContext
{
    int UserId { get; }
    IReadOnlyList<int> RoleIds { get; }
    bool IsAuthenticated { get; }

    /// <summary>Display name or email; null if unknown.</summary>
    string? UserName { get; }

    /// <summary>Role claim (Admin, Agent, Expert, Customer...); null if unknown.</summary>
    string? Role { get; }

    /// <summary>Client IP address; "Unknown" if it cannot be determined.</summary>
    string ClientIp { get; }

    /// <summary>User-Agent header; null if absent.</summary>
    string? UserAgent { get; }

    /// <summary>Whether the user is on the customer (Customer/Company) side.</summary>
    bool IsCustomer { get; }
}

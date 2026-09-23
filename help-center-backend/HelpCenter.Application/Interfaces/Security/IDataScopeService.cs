namespace HelpCenter.Application.Interfaces;

public interface IDataScopeService
{

    /// <summary>
    /// Returns the list of fields/widgets the user can access for the given resource.
    /// If it returns null, all fields are open (no restriction).
    /// If it returns an empty HashSet, no field is accessible.
    /// </summary>
    Task<HashSet<string>?> GetAllowedFieldsAsync(string resourceKey, CancellationToken ct = default);

    /// <summary>
    /// Returns whether the user has the given action permission for the specified resource.
    /// </summary>
    Task<bool> HasPermissionAsync(string resourceKey, string action, CancellationToken ct = default);

    int CurrentUserId { get; }
}

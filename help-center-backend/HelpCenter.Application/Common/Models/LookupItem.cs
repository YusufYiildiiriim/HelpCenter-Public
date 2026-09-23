namespace HelpCenter.Application.Common.Models;

/// <summary>
/// Minimal data shell for UI components such as dropdowns/pickers.
/// Returns only id + display name — carries no sensitive fields (email, role, password, IsActive, etc.).
/// This way a resource's "lookup" permission can be granted separately from "full CRUD" permission.
/// </summary>
public record LookupItem(int Id, string Name);

using HelpCenter.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpCenter.WebApi.Controllers.Admin;

/// <summary>
/// Serves the static metadata needed by the Admin UI.
/// The endpoints are not permission-based; they only require authenticated access —
/// no sensitive data is returned, just structural definitions.
/// </summary>
[Route("api/admin/meta")]
[ApiController]
[Authorize]
public class AdminMetaController : ControllerBase
{
    // Resource + action definitions for rendering the role management screen's checkbox tree.
    // When a new action is added, AppResourceDefinitions is updated; the UI code does not change.
    [HttpGet("resources")]
    public IActionResult Resources()
    {
        return Ok(AppResourceDefinitions.All);
    }

    // Ready-made capability packages (Project Manager, Module Manager, etc.).
    // The UI presents them as presets in the role modal; the selected package is not persisted —
    // a package is just a preset for bulk-filling the action list.
    [HttpGet("features")]
    public IActionResult Features()
    {
        return Ok(FeaturePackages.All);
    }
}

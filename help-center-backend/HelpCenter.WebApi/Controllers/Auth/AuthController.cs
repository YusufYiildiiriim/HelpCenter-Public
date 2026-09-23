using System.Security.Claims;
using HelpCenter.Application.Features.Auth.Commands;
using HelpCenter.Application.Features.Auth.Commands.ChangePassword;
using HelpCenter.Application.Features.Auth.Commands.ForgotPassword;
using HelpCenter.Application.Features.Auth.Commands.ResetPasswordWithCode;
using HelpCenter.Application.Features.Auth.Commands.UserRegister;
using HelpCenter.Application.Features.Auth.Commands.VerifyResetCode;
using HelpCenter.Application.Features.Auth.Queries;
using HelpCenter.Application.Features.Auth.Queries.CustomerLogin;
using HelpCenter.Application.Features.Auth.Queries.GetVerifyState;
using HelpCenter.Application.Features.Auth.Queries.UserLogin;
using HelpCenter.Application.Features.Roles.Queries;
using HelpCenter.Application.Features.Roles.Queries.GetUserPermissions;
using HelpCenter.Application.Interfaces;
using HelpCenter.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace HelpCenter.WebApi.Controllers.Auth;

[Route("api/auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IGenerateJwtToken _jwtTokenGenerator;
    private readonly IMediator _mediator;
    private readonly IUnitOfWork _unitOfWork;

    public AuthController(
        IGenerateJwtToken jwtTokenGenerator,
        IMediator mediator,
        IUnitOfWork unitOfWork)
    {
        _jwtTokenGenerator = jwtTokenGenerator;
        _mediator = mediator;
        _unitOfWork = unitOfWork;
    }

    [HttpPost("admin-login")]
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> AdminLogin([FromBody] UserLoginQuery query)
    {
        var response = await _mediator.Send(query);
        SetRefreshCookie(response.RefreshToken, response.RefreshTokenExpiresAt);
        return Ok(response);
    }

    [HttpPost("customer-login")]
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> CustomerLogin([FromBody] CustomerLoginQuery query)
    {
        var response = await _mediator.Send(query);
        if (!response.Success) return BadRequest(response);
        SetRefreshCookie(response.RefreshToken, response.RefreshTokenExpiresAt);
        return Ok(response);
    }

    [HttpPost("change-password")]
    [AllowAnonymous]
    [EnableRateLimiting("password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordCommand command)
    {
        var response = await _mediator.Send(command);
        return Ok(response);
    }

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    [EnableRateLimiting("password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordCommand command)
    {
        var response = await _mediator.Send(command);
        return Ok(new { success = response, message = "Doğrulama kodu e-posta adresinize gönderildi." });
    }

    [HttpPost("verify-reset-code")]
    [AllowAnonymous]
    [EnableRateLimiting("password")]
    public async Task<IActionResult> VerifyResetCode([FromBody] VerifyResetCodeCommand command)
    {
        var response = await _mediator.Send(command);
        return Ok(new { success = response, message = "Doğrulama kodu onaylandı." });
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    [EnableRateLimiting("password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordWithCodeCommand command)
    {
        var response = await _mediator.Send(command);
        return Ok(new { success = response, message = "Şifreniz başarıyla sıfırlandı." });
    }

    [HttpPost("dashboard-login")]
    [Authorize(Roles = "Admin")]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> DashboardLogin([FromBody] int companyId)
    {
        var company = await _unitOfWork.Repository<Company>().FirstOrDefaultAsync(
            x => x.Id == companyId, cancellationToken: HttpContext.RequestAborted);
        if (company == null) return NotFound();

        var token = _jwtTokenGenerator.GenerateJwtToken("Company", "user", companyId.ToString(), null);

        return Ok(new
        {
            Token = token,
            Role = "Company",
            DashboardUrl = $"/dashboard/{company.PublicId}",
            CompanyPublicId = company.PublicId
        });
    }

    [HttpGet("verify")]
    [Authorize]
    public async Task<IActionResult> Verify()
    {
        var role = User.FindFirst(ClaimTypes.Role)?.Value;
        var username = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? User.FindFirst("sub")?.Value;
        var companyId = User.FindFirst("CompanyId")?.Value;
        var userIdStr = User.FindFirst("UserId")?.Value;

        object? permissions = null;
        bool isPasswordChangeRequired = false;
        int userId = 0;

        if (!string.IsNullOrEmpty(userIdStr) && int.TryParse(userIdStr, out userId))
        {
            var state = await _mediator.Send(new GetVerifyStateQuery(userId));
            isPasswordChangeRequired = state.IsPasswordChangeRequired;
            permissions = state.Permissions;
        }

        return Ok(new
        {
            Authenticated = true,
            Role = role ?? "Guest",
            Username = username ?? "",
            UserId = userId,
            IsPasswordChangeRequired = isPasswordChangeRequired,
            CompanyId = companyId,
            Permissions = permissions
        });
    }

    private void SetRefreshCookie(string refreshToken, DateTime expiresAt)
    {
        if (string.IsNullOrWhiteSpace(refreshToken)) return;

        var options = AuthTokenController.RefreshCookieOptions();
        options.Expires = expiresAt;
        Response.Cookies.Append(AuthTokenController.RefreshTokenCookieName, refreshToken, options);
    }

}

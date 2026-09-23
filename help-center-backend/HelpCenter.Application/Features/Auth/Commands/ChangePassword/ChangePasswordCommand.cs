using MediatR;

namespace HelpCenter.Application.Features.Auth.Commands.ChangePassword;

public class ChangePasswordCommand : IRequest<bool>
{
    public string Email { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
    public bool IsCustomer { get; set; }
}

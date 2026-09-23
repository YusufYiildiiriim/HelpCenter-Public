using MediatR;

namespace HelpCenter.Application.Features.Auth.Commands.ResetPasswordWithCode;

public class ResetPasswordWithCodeCommand : IRequest<bool>
{
    public string EmailOrUsername { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
    public bool IsCustomer { get; set; }
}

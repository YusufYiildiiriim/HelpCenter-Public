using MediatR;

namespace HelpCenter.Application.Features.Auth.Commands.ForgotPassword;

public class ForgotPasswordCommand : IRequest<bool>
{
    public string EmailOrUsername { get; set; } = string.Empty;
    public bool IsCustomer { get; set; }
}

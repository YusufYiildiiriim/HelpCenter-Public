using MediatR;

namespace HelpCenter.Application.Features.Auth.Commands.VerifyResetCode;

public class VerifyResetCodeCommand : IRequest<bool>
{
    public string EmailOrUsername { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public bool IsCustomer { get; set; }
}

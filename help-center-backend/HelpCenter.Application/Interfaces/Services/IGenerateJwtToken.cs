

namespace HelpCenter.Application.Interfaces;

public interface IGenerateJwtToken

{

    public string GenerateJwtToken(string role, string username, string? companyId, string? userId = null);
}

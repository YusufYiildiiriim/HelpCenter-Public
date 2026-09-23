using MediatR;

namespace HelpCenter.Application.Features.Companies.Commands.DeleteCompany;

public record DeleteCompanyCommand(Guid PublicId) : IRequest<bool>;

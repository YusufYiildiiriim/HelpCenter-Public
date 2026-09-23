using AutoMapper;
using HelpCenter.Application.Interfaces;
using HelpCenter.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HelpCenter.Application.Features.Auth.Queries.CustomerLogin;

public class CustomerLoginQueryHandler : IRequestHandler<CustomerLoginQuery, CustomerLoginResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuthTokenService _tokenService;
    private readonly IPasswordService _passwordService;
    private readonly IMapper _mapper;
    private readonly ILogger<CustomerLoginQueryHandler> _logger;

    public CustomerLoginQueryHandler(
        IUnitOfWork unitOfWork,
        IAuthTokenService tokenService,
        IPasswordService passwordService,
        IMapper mapper,
        ILogger<CustomerLoginQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _tokenService = tokenService;
        _passwordService = passwordService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<CustomerLoginResponse> Handle(CustomerLoginQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Müşteri giriş denemesi: {EmailOrUsername}", request.EmailOrUsername);

        var customer = (await _unitOfWork.Repository<Customer>().FindAsync(
            x => x.Account.Email == request.EmailOrUsername || x.Account.Username == request.EmailOrUsername,
            cancellationToken, x => x.Account, x => x.Company)).FirstOrDefault();

        CustomerLoginRules.CustomerShouldExist(customer);
        CustomerLoginRules.CustomerShouldBeActive(customer!);
        CustomerLoginRules.PasswordShouldMatch(_passwordService.VerifyPassword(request.Password, customer!.Account.Password));

        var tokens = await _tokenService.IssueCustomerTokensAsync(
            customer.Id,
            customer.CompanyId,
            customer.Account.Email,
            ipAddress: null,
            userAgent: null,
            cancellationToken);

        var response = _mapper.Map<CustomerLoginResponse>(customer);
        response.Success = true;
        response.Token = tokens.AccessToken;
        response.RefreshToken = tokens.RefreshToken;
        response.RefreshTokenExpiresAt = tokens.RefreshTokenExpiresAt;
        response.CompanyPublicId = customer.Company.PublicId;
        response.CustomerPublicId = customer.PublicId;
        response.Message = "Giriş başarılı.";

        _logger.LogInformation("Müşteri girişi başarılı: {EmailOrUsername}, Müşteri ID: {CustomerId}", request.EmailOrUsername, customer.Id);

        return response;
    }
}

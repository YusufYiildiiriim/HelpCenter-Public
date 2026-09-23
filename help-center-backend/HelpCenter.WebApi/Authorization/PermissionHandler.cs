using System.Security.Claims;
using HelpCenter.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace HelpCenter.WebApi.Authorization
{
    public class PermissionRequirement : IAuthorizationRequirement
    {
        public string Module { get; }
        public string Action { get; }

        public PermissionRequirement(string module, string action)
        {
            Module = module;
            Action = action;
        }
    }

    public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public PermissionHandler(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
        {
            var userIdStr = context.User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
            {
                return;
            }

            using var scope = _scopeFactory.CreateScope();
            var dataScopeService = scope.ServiceProvider.GetRequiredService<IDataScopeService>();

            var hasAccess = await dataScopeService.HasPermissionAsync(requirement.Module, requirement.Action);

            if (hasAccess)
            {
                context.Succeed(requirement);
            }
        }
    }

    public class PermissionPolicyProvider : IAuthorizationPolicyProvider
    {
        public DefaultAuthorizationPolicyProvider FallbackPolicyProvider { get; }

        public PermissionPolicyProvider(IOptions<AuthorizationOptions> options)
        {
            FallbackPolicyProvider = new DefaultAuthorizationPolicyProvider(options);
        }

        public Task<AuthorizationPolicy> GetDefaultPolicyAsync() => FallbackPolicyProvider.GetDefaultPolicyAsync();
        public Task<AuthorizationPolicy?> GetFallbackPolicyAsync() => FallbackPolicyProvider.GetFallbackPolicyAsync();

        public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
        {
            if (policyName.Contains('.'))
            {
                var parts = policyName.Split('.');
                var module = parts[0];
                var action = parts[1];

                var policy = new AuthorizationPolicyBuilder();
                policy.AddRequirements(new PermissionRequirement(module, action));
                return Task.FromResult<AuthorizationPolicy?>(policy.Build());
            }

            return FallbackPolicyProvider.GetPolicyAsync(policyName);
        }
    }
}

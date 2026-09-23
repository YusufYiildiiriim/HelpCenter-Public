using Microsoft.AspNetCore.Authorization;

namespace HelpCenter.WebApi.Attributes
{
    public class HasPermissionAttribute : AuthorizeAttribute
    {
        public string Module { get; }
        public string Action { get; }

        public HasPermissionAttribute(string module, string action) : base(policy: $"{module}.{action}")
        {
            Module = module;
            Action = action;
        }
    }
}

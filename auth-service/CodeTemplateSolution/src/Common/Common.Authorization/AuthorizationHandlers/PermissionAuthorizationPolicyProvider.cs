using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace Common.Authorization.AuthorizationHandlers
{
    public class PermissionAuthorizationPolicyProvider : DefaultAuthorizationPolicyProvider
    {
        public PermissionAuthorizationPolicyProvider(IOptions<AuthorizationOptions> options) : base(options)
        {
        }
        // Treat permissions as policy
        public override async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
        {
            AuthorizationPolicy? authorizationPolicy = await base.GetPolicyAsync(policyName);

            if (authorizationPolicy is not null)
            {
                return authorizationPolicy;
            }

            return new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                //.AddAuthenticationSchemes(CookieAuthenticationDefaults.AuthenticationScheme)
                //.AddRequirements(new PermissionRequirement(policyName))
                .Build();
        }
    }
}

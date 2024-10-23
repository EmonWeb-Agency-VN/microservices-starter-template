using Microsoft.AspNetCore.Authorization;

namespace Common.Authorization.Requirements
{
    public class PermissionRequirement : IAuthorizationRequirement
    {
        public PermissionRequirement(string permission) => Permission = permission;

        /// <summary>
        /// Gets the permission.
        /// </summary>
        public string Permission { get; }
    }
}

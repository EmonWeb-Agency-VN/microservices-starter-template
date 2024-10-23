using Common.Domain.Entities.Roles;
using Microsoft.AspNetCore.Authorization;

namespace Endpoints.Authorization;

[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public sealed class PermissionPolicyAttribute : AuthorizeAttribute
{
    public PermissionPolicyAttribute(PermissionDefinition[] permissions)
    {
        Permissions = permissions;
    }

    public PermissionDefinition[] Permissions { get; }
}

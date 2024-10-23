using Common.Domain.Entities.Users;
using System.Collections.Immutable;

namespace Common.Domain.Entities.Roles
{
    public static class RoleConstants
    {
        public static readonly Guid AdminRoleId = new("38edb058-39ee-48de-8a4d-c4285815b80e");
        public static readonly Guid UserRoleId = new("db269e2e-bea9-480d-b557-3508fc3b9976");

        public static readonly ImmutableList<RolePermission> RolePermissionMappings =
        [
            new RolePermission()
            {
                RoleId = UserRoleId,
                RoleType = RoleType.User,
                Permission = PermissionConstants.UserPermissions,
                UserType = UserType.Internal
            },
            new RolePermission()
            {
                RoleId = AdminRoleId,
                RoleType = RoleType.Admin,
                Permission = PermissionConstants.AdminPermissions,
                UserType = UserType.Internal
            },
        ];
    }
}

using Common.Domain.Entities.Roles;
using Common.Domain.Entities.Users;

namespace Common.SharedKernel.Utilities
{
    public static class UserRolePermissionUtils
    {
        /// <summary>
        /// Get role ids from given role type and user type
        /// </summary>
        /// <param name="roleType"></param>
        /// <param name="userType"></param>
        /// <returns></returns>
        public static List<Guid> GetRoleIdsFromRoleType(RoleType roleType, UserType userType)
        {
            var listRoleIds = new List<Guid>();
            if (userType.HasFlag(UserType.Internal))
            {
                if (roleType != RoleType.None && roleType.HasFlag(RoleType.Admin))
                {
                    var roleId = RoleConstants.RolePermissionMappings.Find(a => a.RoleType == RoleType.Admin)!.RoleId;
                    listRoleIds.Add(roleId);
                }
                if (roleType != RoleType.None && roleType.HasFlag(RoleType.User))
                {
                    var roleId = RoleConstants.RolePermissionMappings.Find(a => a.RoleType == RoleType.User)!.RoleId;
                    listRoleIds.Add(roleId);
                }
            }
            if (userType.HasFlag(UserType.External))
            {
                //Action
            }

            return listRoleIds;
        }
    }
}

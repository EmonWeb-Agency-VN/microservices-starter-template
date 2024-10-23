using Common.Domain.Entities.Users;
using Microsoft.AspNetCore.Http;

namespace Common.SharedKernel.Extensions
{
    public static class HttpContextExtension
    {
        public static Guid CurrentUserId(this HttpContext context)
        {
            var userIdClaim = GetClaimValue(context, "id");
            Guid.TryParse(userIdClaim, out var userId);
            return userId;
        }

        public static RoleType CurrentUserRole(this HttpContext context)
        {
            var userRole = RoleType.None;
            var userRoleValue = GetClaimValue(context, "userrole");
            if (Enum.TryParse(userRoleValue, out RoleType intValue))
            {
                userRole = intValue;
            }
            return userRole;
        }
        private static String GetClaimValue(HttpContext context, String claimType)
        {
            var claimValue = context?.User?.Claims?.FirstOrDefault(s => s.Type.Equals(claimType))?.Value;
            claimValue ??= string.Empty;
            return claimValue;
        }

        public static string CurrentUserName(this HttpContext context)
        {
            return GetClaimValue(context, "username");
        }

        public static string CurrentUserDisplayName(this HttpContext context)
        {
            return GetClaimValue(context, "displayname");
        }

        public static string CurrentExpireTime(this HttpContext context)
        {
            return GetClaimValue(context, "exp");
        }

        public static string CurrentSessionId(this HttpContext context)
        {
            return GetClaimValue(context, CookieClaimConstants.SessionId);
        }

        public static string CurrentSessionExpireTime(this HttpContext context)
        {
            return GetClaimValue(context, CookieClaimConstants.SessionExpireTime);
        }


    }
}

using Common.Domain.Entities.Users;
using Microsoft.AspNetCore.Http;

namespace Common.SharedKernel.Extensions
{
    public static class HttpContextExtension
    {
        public static long CurrentUserId(this HttpContext context)
        {
            var userIdClaim = GetClaimValue(context, "id");
            if (long.TryParse(userIdClaim, out var userId)) return userId;
            else return -1;
        }

        public static long CurrentUserRole(this HttpContext context)
        {
            long userRole = -1;
            var userRoleValue = GetClaimValue(context, "userrole");
            if (long.TryParse(userRoleValue, out long intValue))
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

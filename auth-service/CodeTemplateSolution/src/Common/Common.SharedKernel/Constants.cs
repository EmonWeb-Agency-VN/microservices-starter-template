using Common.Domain.Entities.Users;
using Common.SharedKernel.Utilities;
using System.Text.Json;

namespace Common.SharedKernel
{
    public class Constants
    {
        public const string DevEnv = "Development";
        public const string ASPNETCORE_ENVIRONMENT = "ASPNETCORE_ENVIRONMENT";

        public const string TablePrefix = "Auth";
        public const string EmptyStringFilterValue = "(Empty)";

        public const string AuditObjKey = "AuditObjKey";
        public const string UnknownIP = "UNKNOWN IP";

        public static readonly Guid SystemAdminId = new("c86d9bc1-7d17-4008-a5ab-ba5e5c9810c2");

        public const string DefaultCookieName = "_auth_default";
        public const string DefaultSessionId = "ESSID";

        public static readonly UserEntity SystemAdmin = new()
        {
            Id = SystemAdminId,
            DisplayName = "System Admin",
            UserName = "systemadmin",
            Email = "",
            Mobile = "",
            Gender = Gender.Male,
            Password = PasswordUtils.HashPassword("1qaz2wsxE", out var systemAdminSalt),
            PasswordSalt = systemAdminSalt,
            UserType = UserType.Internal,
            CreatedOn = DateTimeOffset.UtcNow,
            LoginType = LoginType.Email,
            UserStatus = UserStatus.Active
        };

        public static readonly string MobileRegexPattern = @"(((\+|)84)|0)(3|5|7|8|9)+([0-9]{8})\b";

        public static readonly string VietnameseNameRegexPattern = @"^[A-ZÀÁẠẢÃÂẦẤẬẨẪĂẰẮẶẲẴÈÉẸẺẼÊỀẾỆỂỄÌÍỊỈĨÒÓỌỎÕÔỒỐỘỔỖƠỜỚỢỞỠÙÚỤỦŨƯỪỨỰỬỮỲÝỴỶỸĐ][a-zàáạảãâầấậẩẫăằắặẳẵèéẹẻẽêềếệểễìíịỉĩòóọỏõôồốộổỗơờớợởỡùúụủũưừứựửữỳýỵỷỹđ]*(?:[ ][A-ZÀÁẠẢÃÂẦẤẬẨẪĂẰẮẶẲẴÈÉẸẺẼÊỀẾỆỂỄÌÍỊỈĨÒÓỌỎÕÔỒỐỘỔỖƠỜỚỢỞỠÙÚỤỦŨƯỪỨỰỬỮỲÝỴỶỸĐ][a-zàáạảãâầấậẩẫăằắặẳẵèéẹẻẽêềếệểễìíịỉĩòóọỏõôồốộổỗơờớợởỡùúụủũưừứựửữỳýỵỷỹđ]*)*$";

        public static readonly string VietnameseWordRegexPattern = @"[\p{L}\p{M}]";

        public static readonly JsonSerializerOptions JsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        public const string UserSessionCacheKey = "user_session_{0}";
        public const string UserTokenBlacklistCacheKey = "user_token_black_list_{0}";
        public const string SessionExpireCacheKey = "session_expire";
        public const string UserCacheKey = "user_{0}";

        public const string XlsxContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet; charset=utf-8";
        public const string UnknownContentType = "application/octet-stream; charset=utf-8";
        public static readonly Dictionary<TemplateType, string> TemplateMapping = new Dictionary<TemplateType, string>()
        {
            [TemplateType.OrganisationLessonPlan] = @"./ExcelTemplates/OrganisationLessonPlan.xlsx",
        };

        public const string RateLimiterForAnonymous = "anonymous";
        public const string RateLimiterForDefault = "fixed";

    }

    public class AnnonymousEndpoints
    {
        public const string Login = "login";
        public const string Logout = "logout";
    }

    public enum TemplateType
    {
        OrganisationLessonPlan = 0
    }

    public class CityUserCodeRule
    {
        public string CityName { get; set; }
        public string Code { get; set; }
        public string GVCode { get; set; }
        public string TLCode { get; set; }
        public string DSCode { get; set; }
    }
}

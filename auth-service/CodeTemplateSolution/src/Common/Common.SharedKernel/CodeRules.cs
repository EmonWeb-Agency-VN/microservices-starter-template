using Common.Domain.Entities.Users;

namespace Common.SharedKernel
{
    public static class CodeRules
    {
        public static readonly Dictionary<RoleType, string> RoleTypeCode = new()
        {
            [RoleType.None] = "X",
            [RoleType.Admin] = "AD",
            [RoleType.User] = "SA",
        };
    }
}

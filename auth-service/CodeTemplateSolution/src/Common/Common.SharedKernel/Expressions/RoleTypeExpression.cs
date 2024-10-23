using Common.Domain.Entities.Users;
using System.Linq.Expressions;

namespace Common.SharedKernel.Expressions
{
    public class RoleTypeExpression
    {
        public static readonly Expression<Func<UserEntity, bool>> ApplicationAdmin = s => s.UserType.HasFlag(UserType.Internal) && s.RoleType.HasFlag(RoleType.Admin);
    }
}

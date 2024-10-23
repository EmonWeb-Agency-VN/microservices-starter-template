using Common.Domain.Entities.Users;
using System.Linq.Expressions;

namespace Common.SharedKernel.Expressions
{
    public class UserTypeExpression
    {
        public static readonly Expression<Func<UserEntity, bool>> ExternalUser = s => s.UserType.HasFlag(UserType.External);
        public static readonly Expression<Func<UserEntity, bool>> InternalUser = s => s.UserType.HasFlag(UserType.Internal);
    }

}

using Common.Domain.Entities.Roles;

namespace Common.Domain.Entities.Users
{
    public class UserRoleEntity
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public virtual UserEntity User { get; set; }
        public long RoleId { get; set; }
        public virtual RoleEntity Role { get; set; }
    }
}

using Common.Domain.Entities.Roles;

namespace Common.Domain.Entities.Users
{
    public class UserRoleEntity
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public virtual UserEntity User { get; set; }
        public Guid RoleId { get; set; }
        public virtual RoleEntity Role { get; set; }
    }

    public enum ResourceType
    {
        None = 0,
        /// <summary>
        /// Guid.Empty
        /// </summary>
        Global = 1,
        /// <summary>
        /// UserId
        /// </summary>
    }
}

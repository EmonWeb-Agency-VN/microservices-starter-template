using Common.Domain.Entities.Users;

namespace Common.Domain.Entities.Roles
{
    public class RoleEntity
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public RoleClassification RoleClassification { get; set; }
        public RoleLevel RoleLevel { get; set; }
        public virtual ICollection<UserRoleEntity> UserRoleMappings { get; set; } = [];
        public virtual ICollection<RolePermissionEntity> RolePermissions { get; set; } = [];
    }

    public enum RoleClassification
    {
        None = 0,
        BuiltIn = 1,
        Customized = 2
    }

    public enum RoleLevel
    {
        None = 0,
        Internal = 1,
        External = 2,
        System = 3
    }
}

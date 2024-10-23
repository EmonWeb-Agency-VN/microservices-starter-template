namespace Common.Domain.Entities.Roles
{
    public class RolePermissionEntity
    {
        public Guid Id { get; set; }
        public Guid RoleId { get; set; }
        public virtual RoleEntity Role { get; set; }
        public PermissionDefinition Permission { get; set; }
    }
}

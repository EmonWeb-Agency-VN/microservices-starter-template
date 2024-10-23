using Common.Domain.Entities.Users;

namespace Common.Domain.Entities.Roles
{
    public class RolePermission
    {
        public Guid RoleId { get; set; }
        public RoleType RoleType { get; set; }
        public PermissionDefinition[] Permission { get; set; } = [];
        public UserType UserType { get; set; }

        public string PermissionValue
        {
            get
            {
                return string.Join(";", Permission.Select(a => (int)a).OrderBy(a => a));
            }
        }
    }
}

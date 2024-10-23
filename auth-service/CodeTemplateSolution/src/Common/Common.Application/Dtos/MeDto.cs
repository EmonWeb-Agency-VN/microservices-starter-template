using Common.Domain.Entities.Roles;
using Common.Domain.Entities.Users;

namespace Common.Application.Dtos
{
    public class BaseMeDto
    {
        public Guid Id { get; set; }
        public string UserName { get; set; }
        public string DisplayName { get; set; }
        public UserType UserType { get; set; }
        public RoleType RoleType { get; set; }
        public List<PermissionDefinition> Permissions { get; set; }
        public Guid OrganisationId { get; set; }
        public string? Photo { get; set; }
        public bool NeedChangePassword { get; set; }
    }
}

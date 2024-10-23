using Common.Domain.Entities.Users;

namespace Common.Domain.Entities.Roles
{
    [AttributeUsage(AttributeTargets.All)]
    public class RoleDescriptionAttribute : Attribute
    {
        public string Description { get; set; }
        public UserType UserType { get; set; }
        public RoleDescriptionAttribute(string description, UserType userType)
        {
            Description = description;
            UserType = userType;
        }
    }
}

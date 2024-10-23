using Common.Domain.Entities.Users;

namespace Common.Persistence.SeedData.Models
{
    public class UserSeed
    {
        public Guid Id { get; set; }
        public string DisplayName { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public string Code { get; set; }
        public string? Mobile { get; set; }
        public string? Photo { get; set; }
        public string DateOfBirth { get; set; }
        public string Address { get; set; }
        public Gender Gender { get; set; }
        public UserType UserType { get; set; }
        public RoleType RoleType { get; set; }
        public LoginType LoginType { get; set; }
    }
}

using Common.Domain.Entities.Roles;
using System.ComponentModel;

namespace Common.Domain.Entities.Users
{
    public class UserEntity
    {
        public Guid Id { get; set; }
        public string DisplayName { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public string? Mobile { get; set; }
        public DateTimeOffset DateOfBirth { get; set; }
        public Gender Gender { get; set; }
        public UserType UserType { get; set; }
        public RoleType RoleType { get; set; }
        public string Password { get; set; }
        public byte[] PasswordSalt { get; set; } = [];
        public UserStatus UserStatus { get; set; }
        public DateTimeOffset CreatedOn { get; set; }
        public LoginType LoginType { get; set; }
        public bool IsDeleted { get; set; }
        public bool NeedChangePassword { get; set; }

    }
    [Flags]
    public enum RoleType : long
    {
        [RoleDescription("Invalid", UserType.None)]
        None = 0,
        [RoleDescription("", UserType.Internal)]
        Admin = 1,
        [RoleDescription("", UserType.External)]
        User = 2,
    }

    [Flags]
    public enum UserType
    {
        [Description("")]
        None = 0,
        [Description("")]
        Internal = 1,
        [Description("")]
        External = 2,
    }

    public enum Gender
    {
        [Description(description: "Invalid")]
        None = 0,
        [Description("")]
        Male = 1,
        [Description("")]
        Female = 2
    }

    public enum UserStatus
    {
        [Description("Invalid")]
        None = 0,
        [Description("")]
        Active = 1,
        [Description("")]
        Inactive = 2,
        [Description("")]
        Locked = 3,
    }
    [Flags]
    public enum LoginType
    {
        [Description("Invalid")]
        None = 0,
        [Description("Email")]
        Email = 1,
        [Description("Phone Number")]
        Mobile = 2
    }
}

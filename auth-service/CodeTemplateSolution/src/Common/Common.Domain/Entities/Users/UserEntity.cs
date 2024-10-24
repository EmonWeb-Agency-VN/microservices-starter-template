using System.ComponentModel;

namespace Common.Domain.Entities.Users
{
    public class UserEntity
    {
        public long Id { get; set; }
        public string Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string Password { get; set; }
        public byte[] PasswordSalt { get; set; } = [];
        public bool IsEmailVerified { get; set; }
        public bool IsPhoneVerified { get; set; }
        public string OtpCode { get; set; }
        public DateTimeOffset OtpExpiredAt { get; set; }
        public int FailedLoginAttempts { get; set; }
        public DateTimeOffset AccoungLockedUntil { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public DateTimeOffset LastLogin { get; set; }
        public List<UserRoleEntity> UserRoles { get; set; }
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

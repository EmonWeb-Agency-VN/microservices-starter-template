using Common.Domain.Entities.Users;

namespace Common.Domain.Dtos
{
    public class BaseUserInfo
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string? Photo { get; set; }
        public Gender Gender { get; set; }
        public string DateOfBirth { get; set; }
        public string? Email { get; set; }
        public string? Code { get; set; }
        public string? Mobile { get; set; }
        public string? Address { get; set; }
    }

    public class BaseMemberInfo
    {
        public Guid Id { get; set; }
        public Guid ParentId { get; set; }
        public string Name { get; set; }
        public string? Nickname { get; set; }
        public Gender Gender { get; set; }
        public string DateOfBirth { get; set; }
        public string Code { get; set; }
        public string? Photo { get; set; }
        public string? Address { get; set; }
        public string? ParentName { get; set; }
        public string? ParentMobile { get; set; }
    }
}

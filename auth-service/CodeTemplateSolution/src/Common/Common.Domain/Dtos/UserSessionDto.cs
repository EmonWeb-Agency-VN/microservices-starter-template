using Common.Domain.Entities.Users;

namespace Common.Domain.Dtos
{
    public class UserSessionDto
    {
        public Guid MyProperty { get; set; }
        public Guid UserId { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public LoginType LoginType { get; set; }
        public DateTimeOffset LastUpdateTime { get; set; }
        public DateTimeOffset LoginTime { get; set; }
        public DateTimeOffset? LogoutTime { get; set; }
        public bool RememberLogin { get; set; }
    }
}

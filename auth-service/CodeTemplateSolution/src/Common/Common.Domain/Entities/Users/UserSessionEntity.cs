namespace Common.Domain.Entities.Users
{
    public class UserSessionEntity
    {
        public Guid Id { get; set; }
        public long UserId { get; set; }
        public string? Mobile { get; set; }
        public string UserAgent { get; set; }
        public string IpAddress { get; set; }
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public string Roles { get; set; }
        public DateTimeOffset? RefreshTokenExpireTime { get; set; }
        public LoginType LoginType { get; set; }
        public DateTimeOffset LastUpdateTime { get; set; }
        public DateTimeOffset LoginTime { get; set; }
        public DateTimeOffset? LogoutTime { get; set; }
        public bool RememberLogin { get; set; }
    }
}

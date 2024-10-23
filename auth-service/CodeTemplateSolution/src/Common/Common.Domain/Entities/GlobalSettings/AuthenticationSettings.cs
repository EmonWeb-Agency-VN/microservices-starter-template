namespace Common.Domain.Entities.GlobalSettings
{
    public sealed class AuthenticationSettings
    {
        public int DefaultSessionExpireTime { get; set; }
    }

    public sealed class BeforeTimeoutSettings
    {
        public int BeforeTimeout { get; set; }
    }
}

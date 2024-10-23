namespace Common.Domain.Configurations
{
    public class CommonConfig
    {
        public string LoggingDirectory { get; set; }
        public ConnectionStrings ConnectionStrings { get; set; }
        public AuthenticationOptions AuthenticationOptions { get; set; }
        public SSLCertificate SSLCertificate { get; set; }
        public RateLimiter RateLimiter { get; set; }
        public Seq Seq { get; set; }
        public string DeployHost { get; set; }

    }

    public class ConnectionStrings
    {
        public string DbConnection { get; set; }
    }

    public class AuthenticationOptions
    {
        public JwtAuthenticationOptions JwtBearer { get; set; }
        public CookieOptions Cookie { get; set; }
    }

    public class SSLCertificate
    {
        public string Path { get; set; }
    }

    public class JwtAuthenticationOptions
    {
        public int ClockSkew { get; set; }
        public double AccessTokenExpireInMinutes { get; set; }
        public double RefreshTokenExpireInMinutes { get; set; }
        public string ValidIssuer { get; set; }
        public string ValidAudience { get; set; }
        public string SecretKey { get; set; }
    }

    public class CookieOptions
    {
        public int CookieExpireTimeInDays { get; set; }
    }

    public class RateLimiter
    {
        public string Policy { get; set; }
        public int Window { get; set; }
        public int PermitLimit { get; set; }
        public int QueueLimit { get; set; }
    }

    public class Seq
    {
        public string Url { get; set; }
        public string ApiKey { get; set; }
    }
}

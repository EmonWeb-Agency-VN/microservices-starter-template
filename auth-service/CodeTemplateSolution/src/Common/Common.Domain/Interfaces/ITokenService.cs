using System.Security.Claims;

namespace Common.Domain.Interfaces
{
    public interface ITokenService
    {
        string GenerateAccessToken(List<Claim> claims, bool isInnerApi = false);
        string GenerateRefreshToken();
        Task<ClaimsPrincipal> GetPrincipalFromExpiredToken(string token);
        Task<RefreshTokenDto> RefreshToken(RefreshTokenDto model);
        Task<bool> IsInBlacklist(RefreshTokenDto model);
        bool IsExpired(string token);
    }

    public class RefreshTokenDto
    {
        public Guid SessionId { get; set; }
        public string RefreshToken { get; set; }
        public string AccessToken { get; set; }
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            RefreshTokenDto other = (RefreshTokenDto)obj;
            return SessionId == other.SessionId && RefreshToken == other.RefreshToken && AccessToken == other.AccessToken;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(SessionId, AccessToken, RefreshToken);
        }
    }
}

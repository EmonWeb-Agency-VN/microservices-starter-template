using System.Linq.Expressions;
using Common.Domain.Entities.Users;

namespace Common.Domain.Interfaces
{
    public interface IUserSessionService
    {
        Task<UserSessionTokenDto> GetTokenAsync();
        Guid? DecryptSessionId(string? sessionIdStr);
        Task AddOrUpdateUserSessionAsync(UserSessionEntity model, Expression<Func<UserSessionEntity, UserSessionEntity>>? updateFactory = null);
        Task DeleteAllUserSessionAsync(Guid userId);
        Task<Guid> RetreiveSessionId();
        Task BatchDeleteAllUserSessionAsync(List<Guid> userIds);
    }

    public class UserSessionTokenDto
    {
        public Guid Sid { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }
}

using System.Linq.Expressions;
using System.Reflection;
using Common.Domain.Entities.Users;
using Common.Domain.Interfaces;
using Common.SharedKernel;
using Common.SharedKernel.LogProvider;
using Common.SharedKernel.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NLog;

namespace Common.Application.Implementations
{
    public class UserSessionService(IServiceProvider serviceProvider, IHttpContextAccessor contextAccessor) : IUserSessionService
    {
        private static readonly Logger logger = LoggerHelper.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public async Task<UserSessionTokenDto> GetTokenAsync()
        {
            ArgumentNullException.ThrowIfNull(contextAccessor.HttpContext);
            var result = new UserSessionTokenDto();
            if (contextAccessor.HttpContext.Request.Headers.TryGetValue(Constants.DefaultSessionId, out var headerValues))
            {
                var sessionId = headerValues.FirstOrDefault();
                var sid = DecryptSessionId(sessionId);
                if (sid.HasValue && sid != Guid.Empty)
                {
                    var cacheKey = string.Format(Constants.UserSessionCacheKey, sid.Value);
                    var memoryCache = contextAccessor.HttpContext.RequestServices.GetRequiredService<ICustomMemoryCacheService>();
                    var userSession = await memoryCache.GetOrAddAsync(cacheKey, async () =>
                    {
                        var result = await GetUserSessionFromDbAsync(sid.Value);
                        return result;
                    }, TimeSpan.FromDays(1));
                    if (userSession == null) return result;
                    ArgumentNullException.ThrowIfNullOrEmpty(userSession.AccessToken);
                    ArgumentNullException.ThrowIfNullOrEmpty(userSession.RefreshToken);
                    var accessToken = userSession.AccessToken;
                    var refreshToken = userSession.RefreshToken;
                    result.AccessToken = accessToken;
                    result.RefreshToken = refreshToken;
                    result.Sid = sid.Value;
                    return result;

                }
            }
            return result;
        }

        public async Task AddOrUpdateUserSessionAsync(UserSessionEntity model, Expression<Func<UserSessionEntity, UserSessionEntity>>? updateFactory = null)
        {
            var cacheKey = string.Format(Constants.UserSessionCacheKey, model.Id);
            try
            {
                var dbSessionInfo = await GetUserSessionFromDbAsync(model.Id);
                using (var scope = serviceProvider.CreateScope())
                {
                    var dbRepository = scope.ServiceProvider.GetRequiredService<IDBRepository>();
                    var memoryCache = scope.ServiceProvider.GetRequiredService<ICustomMemoryCacheService>();
                    if (dbSessionInfo == null)
                    {
                        await dbRepository.AddAsync(model);
                    }
                    else if (updateFactory is not null)
                    {
                        await dbRepository.UpdateAsync<UserSessionEntity>((e) => e.Id == model.Id, updateFactory);
                    }
                    var result = await dbRepository.SaveChangesAsync();
                    memoryCache.Remove(cacheKey);
                    logger.Info($"Update session item to db. session id: {model.Id}. Result: {result}");
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Error updating user session. Message: {ex.Message}");
                throw;
            }
        }

        public async Task DeleteAllUserSessionAsync(Guid userId)
        {
            try
            {
                using (var scope = serviceProvider.CreateScope())
                {
                    var dbRepository = scope.ServiceProvider.GetRequiredService<IDBRepository>();
                    var memoryCache = scope.ServiceProvider.GetRequiredService<ICustomMemoryCacheService>();
                    var currentSession = await dbRepository.Context.Set<UserSessionEntity>().Where(a => a.UserId == userId).FirstOrDefaultAsync();
                    if (currentSession != null)
                    {
                        dbRepository.Delete(currentSession);
                        await dbRepository.SaveChangesAsync();
                        var cacheKey = string.Format(Constants.UserSessionCacheKey, currentSession.Id);
                        memoryCache.Remove(cacheKey);
                        logger.Info($"Successfully delete sessions of user {userId}");
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Error deleting all user sessions. UserId: {userId}. Message: {ex.Message}");
                throw;
            }
        }
        public async Task BatchDeleteAllUserSessionAsync(List<Guid> userIds)
        {
            try
            {
                using var scope = serviceProvider.CreateScope();
                var dbRepository = scope.ServiceProvider.GetRequiredService<IDBRepository>();
                var memoryCache = scope.ServiceProvider.GetRequiredService<ICustomMemoryCacheService>();
                var currentSessions = await dbRepository.Context.Set<UserSessionEntity>().Where(a => userIds.Contains(a.UserId)).ToListAsync();
                if (currentSessions.Count > 0)
                {
                    dbRepository.DeleteRange(currentSessions);
                    await dbRepository.SaveChangesAsync();
                    currentSessions.ForEach(a =>
                    {
                        var cacheKey = string.Format(Constants.UserSessionCacheKey, a.Id);
                        memoryCache.Remove(cacheKey);
                    });
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Error deleting all user sessions. UserId: {string.Join(";", userIds)}. Message: {ex.Message}");
                throw;
            }
        }

        private async Task<UserSessionEntity?> GetUserSessionFromDbAsync(Guid sessionId)
        {
            try
            {
                using (var scope = serviceProvider.CreateScope())
                {
                    var dbRepository = scope.ServiceProvider.GetRequiredService<IDBRepository>();
                    var dbSessionInfo = await dbRepository.Context.Set<UserSessionEntity>().Where(a => a.Id == sessionId).FirstOrDefaultAsync();
                    if (dbSessionInfo != null)
                    {
                        return dbSessionInfo;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Error getting user session from db. Message: {ex.Message}");
            }
            return null;
        }

        public Guid? DecryptSessionId(string? sessionIdStr)
        {
            if (!string.IsNullOrEmpty(sessionIdStr))
            {
                if (Guid.TryParseExact(sessionIdStr, "N", out var sid))
                {
                    return sid;
                }
                if (Guid.TryParse(AesEncryptionUtil.DecryptStringWithRawKey(sessionIdStr), out var ssid))
                {
                    return ssid;
                }
            }
            return null;
        }

        public async Task<Guid> RetreiveSessionId()
        {
            throw new NotImplementedException();
        }
    }


}

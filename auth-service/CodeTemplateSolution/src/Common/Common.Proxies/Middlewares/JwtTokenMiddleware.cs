using System.Reflection;
using Common.Domain.Interfaces;
using Common.SharedKernel.LogProvider;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using NLog;

namespace Auth.API.Middlewares
{
    public static class JwtTokenMiddleware
    {
        private static readonly Logger logger = LoggerHelper.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly List<string> NoAuthEndpoints = ["/login"];
        public static IApplicationBuilder UseJwtTokenMiddleware(this IApplicationBuilder app)
        {
            return app.Use(async (ctx, next) =>
            {
                var path = ctx.Request.Path;
                if (path.HasValue
                && path.Value.Contains("/open/api", StringComparison.OrdinalIgnoreCase)
                && !NoAuthEndpoints.Any(a => path.Value.Contains(a, StringComparison.OrdinalIgnoreCase))
                && !ctx.Request.Headers.ContainsKey("Authorization"))
                {
                    var sessionService = ctx.RequestServices.GetRequiredService<IUserSessionService>();
                    var tokenService = ctx.RequestServices.GetRequiredService<ITokenService>();
                    var session = await sessionService.GetTokenAsync();
                    var accessToken = session.AccessToken;
                    var refreshToken = session.RefreshToken;
                    var sid = session.Sid;
                    if (!string.IsNullOrEmpty(session.AccessToken))
                    {
                        try
                        {
                            var isExpired = tokenService.IsExpired(accessToken);
                            if (isExpired)
                            {
                                if (!string.IsNullOrEmpty(refreshToken))
                                {
                                    var isInBlacklist = await tokenService.IsInBlacklist(new RefreshTokenDto
                                    {
                                        AccessToken = accessToken,
                                        RefreshToken = refreshToken,
                                        SessionId = sid
                                    });
                                    if (isInBlacklist)
                                    {
                                        logger.Error($"[Refresh token] Token is in blacklist.");
                                        throw new UnauthorizedAccessException();
                                    }
                                    var result = await tokenService.RefreshToken(new RefreshTokenDto
                                    {
                                        AccessToken = accessToken,
                                        RefreshToken = refreshToken,
                                        SessionId = sid
                                    });
                                    accessToken = result.AccessToken;
                                }
                                else
                                {
                                    logger.Error($"No refresh token available.");
                                    throw new UnauthorizedAccessException();
                                }
                            }
                            else
                            {
                                var isInBlacklist = await tokenService.IsInBlacklist(new RefreshTokenDto
                                {
                                    AccessToken = accessToken,
                                    RefreshToken = refreshToken,
                                    SessionId = sid
                                });
                                if (isInBlacklist)
                                {
                                    logger.Error($"[Not expired] Token is in blacklist.");
                                    throw new UnauthorizedAccessException();
                                }
                            }
                            ctx.Request.Headers.Append("Authorization", "Bearer " + accessToken);
                        }
                        catch (Exception ex)
                        {
                            logger.Error(ex, $"Error adding bearer authorization header. Message: {ex.Message}");
                            throw;
                        }
                    }
                }
                await next();
            });
        }
    }
}

using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Common.Domain.Configurations;
using Common.Domain.Entities.Audit;
using Common.Domain.Entities.Tokens;
using Common.Domain.Entities.Users;
using Common.Domain.Enums;
using Common.Domain.Interfaces;
using Common.SharedKernel;
using Common.SharedKernel.Audit;
using Common.SharedKernel.Errors;
using Common.SharedKernel.LogProvider;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NLog;

namespace Common.Application.Implementations
{
    public class TokenService(
        IDBRepository dBRepository,
        IHttpContextAccessor context,
        IUserSessionService sessionService,
        IOptions<AuthenticationOptions> authenticationOptions,
        ICustomMemoryCacheService memoryCache) : ITokenService
    {
        private static readonly Logger logger = LoggerHelper.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public string GenerateAccessToken(List<Claim> claims, bool isInnerApi = false)
        {
            try
            {
                var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authenticationOptions.Value.JwtBearer.SecretKey));
                var signinCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);
                var tokeOptions = new JwtSecurityToken(
                    issuer: authenticationOptions.Value.JwtBearer.ValidIssuer,
                    audience: authenticationOptions.Value.JwtBearer.ValidAudience,
                    claims: claims,
                    expires: DateTime.Now.AddMinutes(isInnerApi ? 5 : authenticationOptions.Value.JwtBearer.AccessTokenExpireInMinutes),
                    signingCredentials: signinCredentials
                );
                var tokenString = new JwtSecurityTokenHandler().WriteToken(tokeOptions);
                return tokenString;

            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Error generating access token. Message: {ex.Message}");
                throw;
            }
        }

        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }

        public async Task<ClaimsPrincipal> GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = true,
                ValidateIssuer = true,
                ValidateIssuerSigningKey = true,
                ValidateLifetime = false,
                ValidIssuer = authenticationOptions.Value.JwtBearer.ValidIssuer,
                ValidAudience = authenticationOptions.Value.JwtBearer.ValidAudience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authenticationOptions.Value.JwtBearer.SecretKey)),
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);
            var jwtSecurityToken = securityToken as JwtSecurityToken;
            if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                logger.Error("Failed to validate token");
                var username = principal.Claims.FirstOrDefault(a => a.Type.Equals("username")).Value;
                var auditModel = new AuditModel(ApiStatus.Failed)
                {
                    UserName = username,
                    AuditAction = AuditAction.TokenValidation,
                    ErrorCode = Error.TokenValidationError.Code,
                    ErrorDetail = "Failed to validate token"
                };
                await context.HttpContext.SetAuditObj(auditModel);
                throw new SecurityTokenException("Invalid token");
            }
            return principal;
        }

        public bool IsExpired(string token)
        {
            var jwtToken = new JwtSecurityTokenHandler().ReadToken(token) as JwtSecurityToken;
            return jwtToken?.ValidTo < DateTime.UtcNow;
        }

        public async Task<bool> IsInBlacklist(RefreshTokenDto model)
        {
            var cacheKey = string.Format(Constants.UserTokenBlacklistCacheKey, model.SessionId);

            var tokenInBls = await memoryCache.GetOrAddAsync(cacheKey, async () =>
            {
                var item = await dBRepository.Context.Set<TokenBlackListEntity>()
                            .Where(a => a.SessionId == model.SessionId)
                            .Select(b => new RefreshTokenDto
                            {
                                SessionId = b.SessionId,
                                AccessToken = b.AccessToken,
                                RefreshToken = b.RefreshToken
                            })
                            .ToListAsync();
                return item;
            }, TimeSpan.FromMinutes(authenticationOptions.Value.JwtBearer.RefreshTokenExpireInMinutes));

            return tokenInBls.Contains(model);
        }

        public async Task<RefreshTokenDto> RefreshToken(RefreshTokenDto model)
        {
            var result = new RefreshTokenDto();
            var userTokenBlCacheKey = string.Format(Constants.UserTokenBlacklistCacheKey, model.SessionId);
            var currentSession = await dBRepository.Context.Set<UserSessionEntity>()
                .Where(a => a.Id == model.SessionId)
                .FirstOrDefaultAsync();
            if (currentSession is not null)
            {
                if (currentSession.RefreshToken != model.RefreshToken)
                {
                    logger.Error("Refresh token not match.");
                    throw new UnauthorizedAccessException();
                }
                if (currentSession.RefreshTokenExpireTime < DateTimeOffset.UtcNow)
                {
                    var existToken = await dBRepository.Context.Set<TokenBlackListEntity>()
                        .AnyAsync(a => a.SessionId == currentSession.Id
                        && currentSession.AccessToken.ToLower() == a.AccessToken.ToLower()
                        && currentSession.RefreshToken.ToLower() == a.RefreshToken.ToLower());
                    if (!existToken)
                    {
                        var expiredToken = new TokenBlackListEntity
                        {
                            SessionId = currentSession.Id,
                            AccessToken = currentSession.AccessToken,
                            RefreshToken = currentSession.RefreshToken
                        };
                        await dBRepository.AddAsync(expiredToken);
                        await dBRepository.SaveChangesAsync();
                        memoryCache.Remove(userTokenBlCacheKey);
                    }
                    logger.Error($"Refresh token already expires.");
                    throw new UnauthorizedAccessException();
                }
                var principal = await this.GetPrincipalFromExpiredToken(model.AccessToken);
                var newAccessToken = this.GenerateAccessToken(principal.Claims.ToList());
                var newRefreshToken = this.GenerateRefreshToken();

                var blItem = new TokenBlackListEntity
                {
                    AccessToken = model.AccessToken,
                    RefreshToken = model.RefreshToken,
                    SessionId = model.SessionId
                };
                await dBRepository.AddAsync(blItem);
                await dBRepository.SaveChangesAsync();

                memoryCache.Remove(userTokenBlCacheKey);

                await sessionService.AddOrUpdateUserSessionAsync(new UserSessionEntity { Id = currentSession.Id },
                                        a => new UserSessionEntity
                                        {
                                            AccessToken = newAccessToken,
                                            RefreshToken = newRefreshToken,
                                            LastUpdateTime = DateTimeOffset.UtcNow,
                                            RefreshTokenExpireTime = DateTimeOffset.UtcNow.AddMinutes(authenticationOptions.Value.JwtBearer.RefreshTokenExpireInMinutes)
                                        });

                result.SessionId = currentSession.Id;
                result.AccessToken = newAccessToken;
                result.RefreshToken = newRefreshToken;
            }
            else
            {
                logger.Error($"Not found current session. SessionId: {model.SessionId}");
            }

            return result;
        }
    }
}

using Common.Domain.Entities.GlobalSettings;
using Common.Domain.Interfaces;
using Common.SharedKernel;
using Common.SharedKernel.LogProvider;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using NLog;
using System.Reflection;
using System.Security.Claims;

namespace Common.Proxies.Authentication
{
    public class CustomCookieAuthenticationEvents : CookieAuthenticationEvents
    {
        private static readonly Logger logger = LoggerHelper.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly List<string> ignoreUpdateCookieExpiresUrl = new List<string> { "checkexpire" };
        public CustomCookieAuthenticationEvents()
        {

        }
        public override async Task SigningIn(CookieSigningInContext context)
        {
            var identity = (ClaimsIdentity)context.Principal.Identity;
            if (identity is null)
            {
                logger.Error("Invalid identity");
                return;
            }
            string username = identity.FindFirst(CookieClaimConstants.UserName)?.Value.ToLower();
            if (string.IsNullOrEmpty(username))
            {
                logger.Error("The username is empty.");
                return;
            }
            SetExpiresTime(identity, context.Properties.ExpiresUtc);
            context.Principal = new ClaimsPrincipal(identity);
        }

        public override async Task ValidatePrincipal(CookieValidatePrincipalContext context)
        {
            var scheme = context.Scheme.Name;
            if (scheme != "Cookies")
            {
                logger.Error($"Ambiguos authentication scheme. Scheme: {scheme}");
                return;
            }
            bool isValidated = false;
            try
            {
                var path = context.Request.Path.Value.ToString().ToLower();
                if (string.IsNullOrEmpty(path))
                {
                    isValidated = true;
                }
                else
                {
                    var username = context.Principal.FindFirst(CookieClaimConstants.UserName)?.Value?.ToString();
                    if (!string.IsNullOrWhiteSpace(username))
                    {
                        var identityClaims = (ClaimsIdentity)context.Principal.Identity;
                        var expireDate = TryGetClaimValue(identityClaims.Claims, CookieClaimConstants.SessionExpireTime, "0");
                        long.TryParse(expireDate, out var expireDateInTicks);
                        DateTimeOffset? expireUtc = new DateTimeOffset(expireDateInTicks, TimeSpan.Zero);
                        if (expireUtc.HasValue && expireUtc.Value >= DateTime.UtcNow)
                        {
                            isValidated = true;
                            if (ignoreUpdateCookieExpiresUrl.Any(u => path.Contains(u, StringComparison.OrdinalIgnoreCase)))
                            {
                                var claimType = CookieClaimConstants.SessionExpireTime;
                                if (identityClaims.HasClaim(c => c.Type == claimType))
                                {
                                    context.Properties.ExpiresUtc = expireUtc;
                                    context.ShouldRenew = true;
                                }
                            }
                            else
                            {
                                var memoryCache = context.HttpContext.RequestServices.GetRequiredService<ICustomMemoryCacheService>();
                                var dbRepository = context.HttpContext.RequestServices.GetRequiredService<IDBRepository>();
                                var cacheKey = Constants.SessionExpireCacheKey;
                                var sessionExpireTime = await memoryCache.GetOrAddAsync(cacheKey, async () =>
                                {
                                    var authenticationSettings = await dbRepository.Context.Set<GlobalSettingsEntity>()
                                            .Where(a => a.Type == GlobalType.AuthenticationSetting)
                                            .Select(a => JsonConvert.DeserializeObject<AuthenticationSettings>(a.Detail))
                                            .FirstOrDefaultAsync();
                                    return authenticationSettings?.DefaultSessionExpireTime;
                                }, TimeSpan.FromDays(1));

                                context.Properties.ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(sessionExpireTime.Value);
                                SetExpiresTime(identityClaims, context.Properties.ExpiresUtc);
                                context.ShouldRenew = true;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Failed to validate pricipal for username. Message: {ex.Message}");
                throw;
            }
            if (!isValidated)
            {
                context.RejectPrincipal();
                await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            }

        }
        public override Task RedirectToLogin(RedirectContext<CookieAuthenticationOptions> context)
        {
            context.Response.StatusCode = 401;
            return Task.CompletedTask;
        }

        public override Task RedirectToAccessDenied(RedirectContext<CookieAuthenticationOptions> context)
        {
            context.Response.StatusCode = 403;
            return Task.CompletedTask;
        }

        private void SetExpiresTime(ClaimsIdentity identity, DateTimeOffset? expireUtc)
        {
            var claimType = CookieClaimConstants.SessionExpireTime;
            if (identity.HasClaim(c => c.Type == claimType))
            {
                var existingClaim = identity.FindFirst(claimType);
                identity.RemoveClaim(existingClaim);
            }
            var newClaim = new Claim(claimType, expireUtc.Value.UtcTicks.ToString());
            identity.AddClaim(newClaim);
        }

        private string TryGetClaimValue(IEnumerable<Claim> claims, string claimType, string defaultValue = "")
        {
            var claimValue = defaultValue;
            var item = claims.FirstOrDefault(x => string.Equals(x.Type, claimType, StringComparison.OrdinalIgnoreCase));
            if (item != null)
            {
                claimValue = item.Value;
            }
            return claimValue;
        }
    }
}

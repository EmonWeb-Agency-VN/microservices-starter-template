using Auth.Service.Commands.Auth;
using Common.Application.Audit;
using Common.Domain.Entities.Audit;
using Common.Domain.Entities.Users;
using Common.Domain.Interfaces;
using Common.Proxies.Controllers;
using Common.SharedKernel;
using Common.SharedKernel.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace Auth.API.Controllers
{
    public class AuthController(
        ISender sender,
        IUserSessionService userSessionService,
        IDBRepository db,
        ICustomMemoryCacheService memoryCache) : ApiBaseController(sender)
    {
        [HttpPost("login")]
        [AllowAnonymous]
        [SwaggerOperation(Summary = "Sign in")]
        [Audit(AuditAction.SignIn)]
        [EnableRateLimiting("anonymous")]
        public async Task<IActionResult> LogIn([FromBody] UserLoginCommand command) => await RunAsync(command);

        [HttpPost("logout")]
        [AllowAnonymous]
        [SwaggerOperation(Summary = "Sign out")]
        [Audit(AuditAction.SignOut)]
        [EnableRateLimiting("anonymous")]
        public async Task<IActionResult> LogOut() => await RunAsync(async () =>
        {
            var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            if (result.Succeeded)
            {
                var currentSessionId = HttpContext.CurrentSessionId();
                var sid = userSessionService.DecryptSessionId(currentSessionId);
                var sessionCacheKey = string.Format(Constants.UserSessionCacheKey, sid);
                var currentSession = await memoryCache.GetOrAddAsync(sessionCacheKey, async () =>
                {
                    var result = await db.Context.Set<UserSessionEntity>().Where(a => a.Id == sid && a.LogoutTime == null).FirstOrDefaultAsync();
                    return result;
                }, TimeSpan.FromMinutes(30));
                if (currentSession != null)
                {
                    currentSession.LogoutTime = DateTimeOffset.UtcNow;
                }
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            }
            return result.Succeeded;
        });

        [HttpGet("checkexpire")]
        [SwaggerOperation(Summary = "Check time before session expire")]
        public async Task<IActionResult> CheckExpire() => await RunAsync(async () =>
        {
            double expireMinutes = 0;
            var expireTimeValue = HttpContext.CurrentSessionExpireTime();
            if (!string.IsNullOrWhiteSpace(expireTimeValue))
            {
                DateTimeOffset? expireUtc = new DateTimeOffset(long.Parse(expireTimeValue), TimeSpan.Zero);
                var expireTime = expireUtc.Value - DateTimeOffset.UtcNow;
                expireMinutes = expireTime.TotalMinutes;
            }

            return expireMinutes < 0 ? 0 : expireMinutes;
        });
    }
}

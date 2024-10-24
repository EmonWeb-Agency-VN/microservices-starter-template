using Common.Application.Errors;
using Common.Application.Messaging;
using Common.Application.ValidationExtensions;
using Common.Domain.Entities.Audit;
using Common.Domain.Entities.GlobalSettings;
using Common.Domain.Entities.Roles;
using Common.Domain.Entities.Users;
using Common.Domain.Enums;
using Common.Domain.Interfaces;
using Common.SharedKernel;
using Common.SharedKernel.Audit;
using Common.SharedKernel.Errors;
using Common.SharedKernel.LogProvider;
using Common.SharedKernel.Utilities;
using FluentValidation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NLog;
using System.Reflection;
using System.Security.Claims;

namespace Auth.Service.Commands.Auth
{
    public class UserLoginCommand : ICommand
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public bool RememberMe { get; set; }
    }

    public class UserLoginCommandValidator : AbstractValidator<UserLoginCommand>
    {
        public UserLoginCommandValidator()
        {
            RuleFor(a => a.UserName).NotEmpty().WithError(UserLoginValidationErrors.UserNameIsRequired);
            RuleFor(a => a.Password).NotEmpty().WithError(UserLoginValidationErrors.PasswordIsRequired);
        }
    }
    internal static class UserLoginErrors
    {
        internal static Error UserNotFound => new("UserLogin.UserNotFound", "Member could not be found.");
        internal static Error InvalidCredentials => new("UserLogin.InvalidCredentials", "The username or password is incorrect.");
        internal static Error UserIsInactive => new("UserLogin.UserIsInactive", "The current account is not activated.");
        internal static Error UserIsDeleted => new("UserLogin.UserIsDeleted", "The current account is deleted.");
        internal static Error UserIsLocked => new("UserLogin.UserIsLocked", "The current account is locked.");
        internal static Error IncorrectPassword => new("UserLogin.IncorrectPassword", "Wrong password.");
    }

    internal static class UserLoginValidationErrors
    {
        internal static Error UserNameIsRequired => new("UserLoginValidation.UserNameIsRequired", "The username field is required");
        internal static Error PasswordIsRequired => new("UserLoginValidation.PasswordIsRequired", "The password field is required");
    }
    internal class UserLoginCommandHandler(
        IDBRepository dBRepository,
        IHttpContextAccessor httpContextAccessor,
        IUserSessionService userSessionService) : ICommandHandler<UserLoginCommand>
    {
        private static readonly Logger logger = LoggerHelper.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public async Task<Result> Handle(UserLoginCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var user = await dBRepository.Context.Set<UserEntity>()
                    .AsNoTracking()
                    .Where(a => a.Email.ToLower() == request.UserName.ToLower()
                                || a.PhoneNumber == request.UserName)
                    .FirstOrDefaultAsync(cancellationToken: cancellationToken);
                if (user == null)
                {
                    await httpContextAccessor.HttpContext.SetAuditObj(new AuditModel(ApiStatus.Failed)
                    {
                        UserName = request.UserName,
                        AuditAction = AuditAction.SignIn,
                        ErrorDetail = UserLoginErrors.UserNotFound.Message,
                        ErrorCode = UserLoginErrors.UserNotFound.Code
                    });
                    logger.Error($"User \"{request.UserName}\" not found");
                    return Result.Failure(UserLoginErrors.InvalidCredentials);
                }
                bool checkPassword = PasswordUtils.VerifyPassword(request.Password, user.Password, user.PasswordSalt);
                if (!checkPassword)
                {
                    await httpContextAccessor.HttpContext.SetAuditObj(new AuditModel(ApiStatus.Failed)
                    {
                        UserName = user.Email,
                        AuditAction = AuditAction.SignIn,
                        ErrorDetail = UserLoginErrors.IncorrectPassword.Message,
                        ErrorCode = UserLoginErrors.IncorrectPassword.Code
                    });
                    logger.Error($"Wrong password for user {request.UserName}");
                    return Result.Failure(UserLoginErrors.InvalidCredentials);
                }
                if (httpContextAccessor.HttpContext is null)
                {
                    return Result.Failure(CommonErrors.ContextIsNull);
                }

                var query = from s in dBRepository.Context.Set<UserRoleEntity>().Where(a => a.UserId == user.Id)
                            select s.RoleId;

                var permissions = await query.OrderBy(a => a).ToListAsync(cancellationToken: cancellationToken);
                var newSession = new UserSessionEntity
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    LastUpdateTime = DateTimeOffset.UtcNow,
                    LoginTime = DateTimeOffset.UtcNow,
                    LoginType = LoginType.Email,
                    RememberLogin = request.RememberMe,
                    UserAgent = httpContextAccessor.HttpContext.Request.Headers.UserAgent.ToString(),
                    IpAddress = httpContextAccessor.HttpContext.Connection.RemoteIpAddress == null ? Constants.UnknownIP : httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString(),
                    Mobile = user.PhoneNumber ?? string.Empty,
                    Roles = string.Join(";", permissions)
                };

                await userSessionService.AddOrUpdateUserSessionAsync(newSession);
                var authenticationSettings = await dBRepository.Context.Set<GlobalSettingsEntity>()
                        .Where(a => a.Type == GlobalType.AuthenticationSetting)
                        .Select(a => new
                        {
                            Detail = JsonConvert.DeserializeObject<AuthenticationSettings>(a.Detail)
                        })
                        .FirstOrDefaultAsync();
                var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
                var sessionExpireTime = authenticationSettings is not null ? authenticationSettings.Detail?.DefaultSessionExpireTime : 30;
                sessionExpireTime ??= 30;
                var encryptedSessionId = AesEncryptionUtil.EncryptStringWithRawKey(newSession.Id.ToString());

                identity.AddClaim(new Claim(CookieClaimConstants.Id, user.Id.ToString()));
                identity.AddClaim(new Claim(CookieClaimConstants.SessionExpireTime, sessionExpireTime.Value.ToString()));
                identity.AddClaim(new Claim(CookieClaimConstants.Email, user.Email));
                identity.AddClaim(new Claim(CookieClaimConstants.Phone, user.PhoneNumber ?? string.Empty));
                identity.AddClaim(new Claim(CookieClaimConstants.SessionId, encryptedSessionId));

                var claimPrincipals = new ClaimsPrincipal(identity);

                await httpContextAccessor.HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimPrincipals, new AuthenticationProperties
                {
                    IsPersistent = request.RememberMe,
                    AllowRefresh = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(sessionExpireTime.Value)
                });

                await httpContextAccessor.HttpContext.SetAuditObj(new AuditModel(ApiStatus.Success)
                {
                    UserName = user.Email,
                    AuditAction = AuditAction.SignIn,
                });

                return Result.Success();

            }
            catch (Exception ex)
            {
                logger.Error(ex, $"[UserLoginCommandHandler] An error ocurred while running command. Message: {ex.Message}");
                throw;
            }
        }
    }
}

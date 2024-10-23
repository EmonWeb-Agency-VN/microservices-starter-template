using Common.Domain.Entities.Roles;
using Common.Domain.Entities.Users;
using Common.Domain.Interfaces;
using Common.SharedKernel;
using Common.SharedKernel.Attributes;
using Common.SharedKernel.Extensions;
using Common.SharedKernel.LogProvider;
using Common.SharedKernel.Utilities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using System.Reflection;

namespace Common.Authorization.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public sealed class HasPermissionAttribute : Attribute, IAsyncActionFilter
    {
        private static readonly Logger _logger = LoggerHelper.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly PermissionDefinition[] requiredPermissions;
        private readonly bool isSystemAdmin;
        private readonly ResourceType resourceType;
        private readonly string? resourceParamName;
        private readonly string? resourceIdPath;

        public HasPermissionAttribute(params PermissionDefinition[] requiredPermissions)
        {
            this.requiredPermissions = requiredPermissions;
        }
        public HasPermissionAttribute(bool isSystemAdmin)
        {
            this.isSystemAdmin = isSystemAdmin;
        }
        public HasPermissionAttribute(PermissionDefinition[] requiredPermissions, ResourceType resourceType = ResourceType.Global, string? resourceParamName = null, string? resourceIdPath = null)
        {
            this.requiredPermissions = requiredPermissions;
            this.resourceType = resourceType;
            this.resourceParamName = resourceParamName;
            this.resourceIdPath = resourceIdPath;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            await OnActionExecuting(context);
            await next();
        }
        public async Task OnActionExecuting(ActionExecutingContext context)
        {
            try
            {
                var currentPath = context.HttpContext.Request.Path.ToString().ToLower();
                if (!currentPath.Contains("/portal/api", StringComparison.OrdinalIgnoreCase) && !currentPath.Contains("/open/api", StringComparison.OrdinalIgnoreCase)) return;
                if (requiredPermissions == null || requiredPermissions.Length == 0)
                {
                    return;
                }
                var currentUserId = context.HttpContext.CurrentUserId();
                if (currentUserId == Guid.Empty)
                {
                    OnActionFailed();
                    return;
                }
                var currentSessionId = context.HttpContext.CurrentSessionId();
                var userSessionService = context.HttpContext.RequestServices.GetRequiredService<IUserSessionService>();
                var sid = userSessionService.DecryptSessionId(currentSessionId);
                var db = context.HttpContext.RequestServices.GetRequiredService<IDBRepository>();
                var memoryCache = context.HttpContext.RequestServices.GetRequiredService<ICustomMemoryCacheService>();
                var cacheKey = string.Format(Constants.UserCacheKey, currentUserId);
                var currentUserRole = await memoryCache.GetOrAddAsync(cacheKey, async () =>
                {
                    var result = await db.Context.Set<UserRoleEntity>()
                    .Include(a => a.User)
                    .Where(a => a.UserId == currentUserId).Select(a => new { a.User.RoleType, a.User.UserStatus, a.Id }).FirstOrDefaultAsync();
                    return result;
                }, TimeSpan.FromMinutes(30));
                if (currentUserRole == null)
                {
                    _logger.Warn($"User not found. Id: {currentUserId}");
                    OnActionFailed();
                    return;
                }
                if (currentUserRole.UserStatus == UserStatus.Inactive || currentUserRole.UserStatus == UserStatus.Locked)
                {
                    _logger.Warn($"User is inactive or locked. Status: {currentUserRole.UserStatus.ToString()}");
                    OnActionFailed();
                }
                var sessionCacheKey = string.Format(Constants.UserSessionCacheKey, sid);
                var currentSession = await memoryCache.GetOrAddAsync(sessionCacheKey, async () =>
                {
                    var result = await db.Context.Set<UserSessionEntity>().Where(a => a.Id == sid && a.LogoutTime == null).FirstOrDefaultAsync();
                    return result;
                }, TimeSpan.FromMinutes(30));
                if (currentSession == null)
                {
                    _logger.Warn($"Session not found. SID: {sid}");
                    await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                    return;
                }
                if (this.isSystemAdmin || currentUserRole.RoleType == RoleType.Admin)
                {
                    _logger.Info($"Skip checking permission. Current user is System admin.");
                    return;
                }

                var permissions = currentSession.Permissions.Split(";").Select(a => (PermissionDefinition)int.Parse(a)).ToList();
                var hasPermissions = permissions.Intersect(requiredPermissions).Any();
                if (!hasPermissions)
                {
                    _logger.Warn($"User don't have required permissions.");
                    OnActionFailed();
                    return;
                }
                else if (!currentUserRole.RoleType.HasFlag(RoleType.User))
                {
                    var resourceIds = GetResourceIds(context);
                    //if (resourceIds.Any())
                    //{
                    //    var userScopeIds = await db.Context.Set<UserRoleResourceMappingEntity>()
                    //        .Where(a => a.UserRoleId == currentUserRole.Id
                    //        && resourceType == a.ResourceType
                    //        && resourceIds.Contains(a.ResourceId))
                    //        .Select(a => a.ResourceId).ToListAsync();
                    //    if (!userScopeIds.Intersect(resourceIds).Any())
                    //    {
                    //        OnActionFailed();
                    //    }
                    //}
                }

            }
            catch (Exception ex)
            {
                _logger.Error($"Error checking permission for action. Message: {ex.Message}", ex);
                throw;
            }

        }
        private void OnActionFailed()
        {
            _logger.Error($"Access denied.");
            throw new AccessDeniedException();
        }

        private List<Guid> GetResourceIds(ActionExecutingContext context)
        {
            var resourceIds = new List<Guid>();
            try
            {
                if (resourceParamName != null)
                {
                    if (context.ActionArguments.Keys.Contains(resourceParamName))
                    {
                        object outObj = context.ActionArguments[resourceParamName];

                        object tempResourceId;

                        if (string.IsNullOrEmpty(this.resourceIdPath))
                        {
                            tempResourceId = outObj;
                        }
                        else
                        {
                            tempResourceId = ValueReaderUtil.ReadValue(outObj, this.resourceIdPath);
                        }

                        if (tempResourceId != null)
                        {
                            var guids = tempResourceId as List<Guid>;
                            if (guids != null)
                            {
                                resourceIds = guids;
                            }
                            else
                            {
                                Guid tempId;
                                if (Guid.TryParse(tempResourceId.ToString(), out tempId))
                                {
                                    resourceIds.Add(tempId);
                                }
                                else
                                {
                                    _logger.Warn("can not parse param to guid");
                                }
                            }
                        }
                    }
                    else
                    {
                        throw new Exception("param not found!");
                    }
                }
            }
            catch (Exception e)
            {
                _logger.Warn($"GetResourceIds Exception. Message: {e.Message}", e);
                throw;
            }

            return resourceIds;
        }
    }
}


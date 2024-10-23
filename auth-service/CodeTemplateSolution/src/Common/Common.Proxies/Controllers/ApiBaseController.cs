using Common.Application.Audit;
using Common.Application.Messaging;
using Common.Domain.Entities.Audit;
using Common.Domain.Enums;
using Common.Domain.Filters;
using Common.SharedKernel.Attributes;
using Common.SharedKernel.Audit;
using Common.SharedKernel.Extensions;
using Common.SharedKernel.LogProvider;
using MediatR;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using NLog;
using Swashbuckle.AspNetCore.Annotations;
using System.Reflection;

namespace Common.Proxies.Controllers
{
    [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme, Policy = "Auth")]
    [Route("portal/api/[controller]")]
    [ApiExceptionResult]
    [ApiController]
    [ApiResponseResult]
    [Produces("application/json")]
    [ApiExplorerSettings(GroupName = "Auth")]
    [SwaggerResponse(200, Type = typeof(Result))]
    [EnableRateLimiting("fixed")]
    public class ApiBaseController(ISender sender) : ControllerBase
    {
        private static readonly Logger logger = LoggerHelper.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        /// <summary>
        /// Normal query
        /// </summary>
        /// <typeparam name="TQueryResult"></typeparam>
        /// <param name="query"></param>
        /// <returns></returns>
        protected async Task<IActionResult> RunAsync<TQueryResult>(IQuery<TQueryResult> query) where TQueryResult : new()
        {
            return await HandleRequestAsync(async () =>
            {
                var result = await sender.Send(query);
                return Ok(result);
            });
        }
        /// <summary>
        /// Normal query with String return type
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        protected async Task<IActionResult> RunAsync(IQuery<string> query)
        {
            return await HandleRequestAsync(async () =>
            {
                var result = await sender.Send(query);
                return Ok(result);
            });
        }
        /// <summary>
        /// Filter query
        /// </summary>
        /// <typeparam name="TFilterQuery"></typeparam>
        /// <typeparam name="TQueryResult"></typeparam>
        /// <param name="query"></param>
        /// <returns></returns>
        protected async Task<IActionResult> RunAsync<TFilterQuery, TQueryResult>(TFilterQuery query)
            where TQueryResult : new()
            where TFilterQuery : FilterModel, IQuery<FilterResult<List<TQueryResult>>>
        {
            return await HandleRequestAsync(async () =>
            {
                var result = await sender.Send(query);
                return Ok(result);
            });
        }
        /// <summary>
        /// Normal command with String return type
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        protected async Task<IActionResult> RunAsync(ICommand<string> command)
        {
            return await HandleRequestAsync(async () =>
            {
                var result = await sender.Send(command);
                return Ok(result);
            });
        }
        /// <summary>
        /// Normal command with return type
        /// </summary>
        /// <typeparam name="TCommandResult"></typeparam>
        /// <param name="command"></param>
        /// <returns></returns>
        protected async Task<IActionResult> RunAsync<TCommandResult>(ICommand<TCommandResult> command)
            where TCommandResult : new()
        {
            return await HandleRequestAsync(async () =>
            {
                var result = await sender.Send(command);
                return Ok(result);
            });
        }
        /// <summary>
        /// Normal command
        /// </summary>
        /// <typeparam name="TCommand"></typeparam>
        /// <param name="command"></param>
        /// <returns></returns>
        protected async Task<IActionResult> RunAsync<TCommand>(TCommand command) where TCommand : ICommand
        {
            return await HandleRequestAsync(async () =>
            {
                var result = await sender.Send(command);
                return Ok(result);
            });
        }
        /// <summary>
        /// Normal function (not query or command)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="function"></param>
        /// <returns></returns>
        protected async Task<IActionResult> RunAsync<T>(Func<Task<T>> function)
        {
            return await HandleRequestAsync(async () =>
            {
                var result = await function.Invoke();
                return Ok(result);
            });
        }

        private async Task<IActionResult> HandleRequestAsync(Func<Task<IActionResult>> action)
        {
            var (controllerName, actionName, currentUserName, auditAction) = GetActionMethodValue();
            try
            {
                var result = await action.Invoke();
                if (auditAction != AuditAction.None && auditAction != AuditAction.SignIn)
                {
                    await HttpContext.SetAuditObj(new AuditModel(ApiStatus.Success)
                    {
                        UserName = currentUserName,
                        AuditAction = auditAction,
                    });
                }
                return result;
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"An error occurred in {controllerName}/{actionName}. Message: {ex.Message}");
                if (auditAction != AuditAction.None)
                {
                    await HttpContext.SetAuditObj(new AuditModel(ApiStatus.Failed)
                    {
                        UserName = currentUserName,
                        AuditAction = auditAction,
                        ErrorDetail = $"Controller: {controllerName}/{actionName}. Action: {auditAction.ToDescription()}. Message: {ex}",
                    });
                }
                else
                {
                    await HttpContext.SetAuditObj(new AuditModel(ApiStatus.Failed)
                    {
                        UserName = currentUserName,
                        AuditAction = auditAction,
                        ErrorDetail = $"Controller: {controllerName}/{actionName}. Message: {ex}",
                    });
                }

                throw;
            }
        }

        private (string, string, string, AuditAction) GetActionMethodValue()
        {
            var controllerName = RouteData.Values["controller"]?.ToString();
            var actionName = RouteData.Values["action"]?.ToString();

            var currentUserName = HttpContext.CurrentUserName();

            var actionContext = ControllerContext.ActionDescriptor;
            var methodInfo = GetType().GetMethod(actionContext.ActionName);
            var auditAttribute = methodInfo?.GetCustomAttribute<AuditAttribute>();
            var auditAction = auditAttribute?.AuditAction ?? AuditAction.None;

            return (controllerName, actionName, currentUserName, auditAction);
        }
    }
}

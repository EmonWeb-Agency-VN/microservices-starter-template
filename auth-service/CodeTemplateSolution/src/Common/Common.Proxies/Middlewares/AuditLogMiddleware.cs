using Common.Application.Time;
using Common.Domain.Entities.Audit;
using Common.Domain.Interfaces;
using Common.SharedKernel;
using Common.SharedKernel.Extensions;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using System.Text;

namespace Common.Proxies.Middlewares
{
    public class AuditLogMiddleware
    {
        private const string ControllerKey = "controller";
        private readonly RequestDelegate _next;
        public AuditLogMiddleware(RequestDelegate next)
        {
            _next = next;
        }
        public async Task InvokeAsync(HttpContext context, IDBRepository dBRepository, ISystemTime systemTime)
        {
            await _next(context);
            var request = context.Request;
            if (request.Path.StartsWithSegments("/open/api") || request.Path.StartsWithSegments("/portal/api"))
            {
                request.RouteValues.TryGetValue(ControllerKey, out var controllerValue);
                var controllerName = (string)(controllerValue ?? string.Empty);
                var changedValue = await GetChangedValues(request).ConfigureAwait(false);
                if (context.Items.ContainsKey(Constants.AuditObjKey))
                {
                    if (context.Items[Constants.AuditObjKey] is not AuditModel model) return;

                    var auditLog = new AuditEntity
                    {
                        Username = !string.IsNullOrEmpty(model.UserName) ? model.UserName : context.CurrentUserName(),
                        IpAddress = context.Connection.RemoteIpAddress == null ? Constants.UnknownIP : context.Connection.RemoteIpAddress.ToString(),
                        UserAgent = context.Request.Headers["User-Agent"].ToString(),
                        EntityName = controllerName,
                        Description = model.AuditAction.ToDescription(),
                        AuditAction = model.AuditAction,
                        Method = request.Method,
                        Timestamp = systemTime.UtcNow,
                        ObjectInfo = changedValue,
                        Status = model.Status,
                        Error = model.ErrorDetail,
                        ErrorCode = model.ErrorCode,
                        ApiEndpoint = request.Path.HasValue ? request.Path.Value : string.Empty
                    };
                    await dBRepository.AddAsync(auditLog);
                    await dBRepository.SaveChangesAsync();
                }
            }
        }

        private static async Task<string> GetChangedValues(HttpRequest request)
        {
            StringBuilder changedValue = new StringBuilder(string.Empty);
            switch (request.Method)
            {
                case "POST":
                case "PUT":
                    var result = await ReadRequestBody(request, Encoding.UTF8).ConfigureAwait(false);
                    changedValue.Append(result);
                    break;
                case "GET":
                case "DELETE":
                    var items = request.RouteValues
                        .Where(a => a.Key != "action" && a.Key != "controller")
                        .ToList();
                    items.ForEach(i =>
                    {
                        var change = i.Value ?? string.Empty;
                        changedValue.Append($"{i.Key}:{change};");
                    });
                    break;
                default:
                    break;
            }
            return changedValue.ToString();
        }
        private static async Task<string> ReadRequestBody(HttpRequest request, Encoding? encoding = null)
        {
            request.Body.Position = 0;
            if (request.Path.HasValue && request.Path.Value.Contains("/api/Image/bulkupload"))
            {
                var result = string.Join("; ", request.Form.Files.Select(a => a.FileName).ToList());
                request.Body.Position = 0;
                return result.Replace("\n", "").Replace("\r", "");
            }
            var reader = new StreamReader(request.Body, encoding ?? Encoding.UTF8);
            var requestBody = await reader.ReadToEndAsync().ConfigureAwait(false);
            if (request.Path.HasValue && (
                request.Path.Value.Contains("/portal/api/Auth/login") ||
                request.Path.Value.Contains("/portal/api/Account/create") ||
                request.Path.Value.Contains("/open/api/Auth/signin") ||
                request.Path.Value.Contains("/open/api/Auth/addsauser")))
            {
                JObject json = JObject.Parse(requestBody);
                json["password"] = "****";
                string result = json.ToString();
                request.Body.Position = 0;
                return result.Replace("\n", "").Replace("\r", "");
            }
            request.Body.Position = 0;
            return requestBody.Replace("\n", "").Replace("\r", "");
        }
    }
}

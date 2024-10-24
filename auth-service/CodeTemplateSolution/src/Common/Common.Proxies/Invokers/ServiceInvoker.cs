using Common.Domain.Interfaces;
using Common.Proxies.Attributes;
using Common.Proxies.Interfaces;
using Common.SharedKernel.Extensions;
using Grpc.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq.Expressions;
using System.Net.Http.Headers;
using System.Reflection;
using System.Security.Claims;
using System.Text;

namespace Common.Proxies.Invokers
{
    public class ServiceInvoker
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor contextAccessor;
        private readonly IServiceScopeFactory serviceScopeFactory;
        private readonly string _baseUrl;

        public ServiceInvoker(
            IHttpClientFactory httpClientFactory,
            IConfiguration config,
            IHttpContextAccessor contextAccessor,
            IServiceScopeFactory serviceScopeFactory)
        {
            this._httpClientFactory = httpClientFactory;
            this.contextAccessor = contextAccessor;
            this.serviceScopeFactory = serviceScopeFactory;
            this._baseUrl = config["DeployHost"];
        }

        public async Task<TResult> InvokeAsync<TRequest, TResult>(Expression<Func<TRequest, Task<TResult>>> expression)
        {
            var methodCall = (MethodCallExpression)expression.Body;
            var methodInfo = methodCall.Method;

            // Xử lý HTTP methods
            var controllerAttr = typeof(TRequest).GetCustomAttribute<ControllerEndpointAttribute>() ?? throw new InvalidOperationException("Missing ControllerEndpoint attribute.");
            var proxyMethod = methodInfo.GetCustomAttribute<ProxyMethodAttribute>();


            if (proxyMethod != null)
            {
                return await InvokeMethodHttpAsync<TResult>(methodCall, proxyMethod.Method, controllerAttr.Controller, proxyMethod.Action);
            }

            throw new InvalidOperationException("No valid HTTP or gRPC method attribute found.");
        }

        public async Task<TResult> InvokeMethodHttpAsync<TResult>(MethodCallExpression methodCall, string httpMethod, string controller, string actionPath)
        {
            using var httpClient = _httpClientFactory.CreateClient();
            var fullUrl = $"{_baseUrl}/{controller}/{actionPath}";
            var arguments = methodCall.Arguments.Select(a => Expression.Lambda(a).Compile().DynamicInvoke()).ToArray();
            for (int i = 0; i < arguments.Length; i++)
            {
                fullUrl = fullUrl.Replace($"{{{i}}}", arguments[i]?.ToString());
            }
            var scope = serviceScopeFactory.CreateScope();
            var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();
            var userId = contextAccessor.HttpContext.CurrentUserId();
            var claims = new List<Claim>
                {
                    new ("id", userId.ToString()),
                };
            var accessToken = tokenService.GenerateAccessToken(claims, isInnerApi: true);

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            HttpRequestMessage request = new(new HttpMethod(httpMethod), fullUrl);

            if (httpMethod.EqualsIgnoreCase(ProxyConstants.HttpPost))
            {
                var body = arguments.Last();
                request.Content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");
            }

            var response = await httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadAsStringAsync();
            JObject json = JObject.Parse(result);
            var data = json["result"].ToString();
            return JsonConvert.DeserializeObject<TResult>(data);
        }
    }
}

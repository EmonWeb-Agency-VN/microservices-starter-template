using Common.SharedKernel.LogProvider;
using Microsoft.AspNetCore.Http;
using NLog;
using System.Diagnostics;
using System.Reflection;

namespace Auth.API.Middlewares
{
    public class RequestTimingMiddleware(RequestDelegate next)
    {
        private static readonly Logger _logger = LoggerHelper.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public async Task Invoke(HttpContext context)
        {
            var stopWatch = Stopwatch.StartNew();
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                _logger.Error($"An error occur while running RequestTimingMiddleware. Message: {ex.Message}. InnerException: {ex.InnerException?.Message}", ex);
                throw;
            }
            finally
            {
                stopWatch.Stop();
                var ms = stopWatch.ElapsedMilliseconds;
                _logger.Info($"HttpRequest {context.Request.Path} takes {ms}ms in RequestTimingMiddleware");
            }
        }
    }
}

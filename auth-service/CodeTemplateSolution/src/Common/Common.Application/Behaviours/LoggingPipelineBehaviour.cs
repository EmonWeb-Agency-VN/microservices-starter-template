using System.Reflection;
using Common.SharedKernel.LogProvider;
using MediatR;
using NLog;

namespace Common.Application.Behaviours
{
    public class LoggingPipelineBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
    {
        private static readonly Logger _logger = LoggerHelper.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            _logger.Info($"Handling {typeof(TRequest).Name}");
            var response = await next();

            _logger.Info($"Handled {typeof(TResponse).Name}");

            return response;
        }
    }
}

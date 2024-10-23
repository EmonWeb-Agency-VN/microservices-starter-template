using Common.Application.Infrastructure;
using Common.Application.Messaging;
using Common.SharedKernel.Errors;
using Common.SharedKernel.LogProvider;
using FluentValidation;
using MediatR;
using NLog;
using System.Reflection;
using System.Text;

namespace Common.Application.Behaviours
{
    public sealed class ValidationPipelineBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse> where TResponse : Result
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;
        private static readonly Logger _logger = LoggerHelper.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public ValidationPipelineBehavior(IEnumerable<IValidator<TRequest>> validators) => _validators = validators;
        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            if (!_validators.Any())
            {
                return await next();
            }

            var context = new ValidationContext<TRequest>(request);
            Error[] errors = await ValidateAsync(context);

            if (errors.Length != 0)
            {
                StringBuilder sb = new();
                foreach (var error in errors)
                {
                    sb.AppendLine($"{error.Code.ToString()} - {error.Message}");
                }
                _logger.Error($"Validation errors: {sb.ToString()}");
                return ValidationResultFactory.Create<TResponse>(errors);
            }
            return await next();
        }

        private async Task<Error[]> ValidateAsync(IValidationContext validationContext)
        {
            var validationResults = await Task.WhenAll(_validators.Select((validator) => validator.ValidateAsync(validationContext)));
            var failures = validationResults
                    .SelectMany(validationResult => validationResult.Errors)
                    .Where(validationFailure => validationFailure is not null)
                    .Select(validationFailure => new Error(validationFailure.ErrorCode, validationFailure.ErrorMessage))
                    .Distinct()
                    .ToArray();

            return failures;
        }
    }
}

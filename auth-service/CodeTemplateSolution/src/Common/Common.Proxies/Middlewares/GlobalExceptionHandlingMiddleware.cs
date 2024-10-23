using Common.Domain.Enums;
using Common.SharedKernel;
using Common.SharedKernel.ApiResponse;
using Common.SharedKernel.Attributes;
using Microsoft.AspNetCore.Http;

namespace Auth.API.Middlewares
{
    public class GlobalExceptionHandlingMiddleware(RequestDelegate next)
    {
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            ApiResponseDto response;
            int statusCode;

            if (exception is UnauthorizedAccessException)
            {
                statusCode = StatusCodes.Status401Unauthorized;
                response = new ApiResponseDto
                {
                    Code = statusCode,
                    Status = ApiStatus.Failed,
                    Message = "You are not authorized to access this resource."
                };
            }
            else if (exception is AccessDeniedException)
            {
                statusCode = StatusCodes.Status403Forbidden;
                response = new ApiResponseDto
                {
                    Code = statusCode,
                    Status = ApiStatus.Failed,
                    Message = "You don't have access to this resource."
                };
            }
            // Handle other exceptions
            else
            {
                statusCode = StatusCodes.Status500InternalServerError;
                response = new ApiResponseDto
                {
                    Code = statusCode,
                    Status = ApiStatus.Failed,
                    Message = "An unexpected error occurred."
                };
            }

            context.Response.StatusCode = statusCode;

            return context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(response, Constants.JsonSerializerOptions));
        }
    }
}

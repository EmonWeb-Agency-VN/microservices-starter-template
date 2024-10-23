using Common.Domain.Enums;
using Common.SharedKernel.ApiResponse;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.ComponentModel.DataAnnotations;

namespace Common.SharedKernel.Attributes
{
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
    public class ApiExceptionResultAttribute : ExceptionFilterAttribute
    {
        public override void OnException(ExceptionContext context)
        {
            base.OnException(context);

            if (context.Exception is UnauthorizedAccessException)
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Result = new ObjectResult(
                    new ApiResponseDto
                    {
                        Code = StatusCodes.Status401Unauthorized,
                        Status = ApiStatus.Failed,
                        Message = "You are not authorized to access this resource."
                    });
            }
            else if (context.Exception is AccessDeniedException)
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
                context.Result = new ObjectResult(
                    new ApiResponseDto
                    {
                        Code = StatusCodes.Status403Forbidden,
                        Status = ApiStatus.Failed,
                        Message = "You don't have access to this resource"
                    });
            }
            else if (context.Exception is NotAcceptedException)
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status406NotAcceptable;
                context.Result = new ObjectResult(
                    new ApiResponseDto
                    {
                        Code = StatusCodes.Status406NotAcceptable,
                        Status = ApiStatus.Failed,
                        Message = "The server cannot generate a response that is acceptable according to the request \"Accept\" headers."
                    });
            }
            else if (context.Exception is BadRequestException)
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                context.Result = new ObjectResult(
                    new ApiResponseDto
                    {
                        Code = StatusCodes.Status400BadRequest,
                        Status = ApiStatus.Failed,
                        Message = "The server cannot process the request due to a client error."
                    });
            }
            else if (context.Exception is Error500Exception)
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
                context.Result = new ObjectResult(
                    new ApiResponseDto
                    {
                        Code = StatusCodes.Status500InternalServerError,
                        Status = ApiStatus.Failed,
                        Message = "An unexpected error occurred. Please try again later."
                    });
            }
            else if (context.Exception is Error404Exception)
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status404NotFound;
                context.Result = new ObjectResult(
                    new ApiResponseDto
                    {
                        Code = StatusCodes.Status404NotFound,
                        Status = ApiStatus.Failed,
                        Message = "The requested resource was not found."
                    });
            }
            else if (context.Exception is ValidationException)
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                context.Result = new ObjectResult(
                    new ApiResponseDto
                    {
                        Code = StatusCodes.Status400BadRequest,
                        Status = ApiStatus.Failed,
                        Message = context.Exception.Message
                    });
            }
            else
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
                context.Result = new ObjectResult(
                    new ApiResponseDto
                    {
                        Code = StatusCodes.Status500InternalServerError,
                        Status = ApiStatus.Failed,
                        Message = context.Exception.Message
                    });
            }

            context.ExceptionHandled = true;
        }
    }

    public class CustomException : Exception
    {
        public CustomException() : base()
        {

        }
        public CustomException(string message) : base(message)
        {

        }
        public CustomException(string message, Exception innerException) : base(message, innerException)
        {

        }
    }

    public class AccessDeniedException : CustomException
    {
        public AccessDeniedException()
        {

        }
        public AccessDeniedException(string message) : base(message)
        {

        }
    }

    public class Error500Exception : CustomException
    {
        public Error500Exception(string message) : base(message)
        {
        }
    }

    public class BadRequestException : CustomException
    {
        public BadRequestException(string message) : base(message)
        {
        }
    }

    public class Error404Exception : CustomException
    {
        public Error404Exception(string message) : base(message)
        {
        }
    }

    public class NotExistException : CustomException
    {
        public NotExistException(string sourceName) : base($"The {sourceName} doesn't exist.")
        {
        }
    }

    public class NotAcceptedException : CustomException
    {
        public NotAcceptedException()
        {

        }
        public NotAcceptedException(string message) : base(message)
        {

        }
    }
}

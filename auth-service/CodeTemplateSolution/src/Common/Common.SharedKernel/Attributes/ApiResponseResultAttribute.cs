using System.Reflection;
using System.Text;
using System.Web;
using Common.Domain.Enums;
using Common.SharedKernel.ApiResponse;
using Common.SharedKernel.LogProvider;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using NLog;

namespace Common.SharedKernel.Attributes
{
    public class ApiResponseResultAttribute : ActionFilterAttribute
    {
        private static readonly Logger _logger = LoggerHelper.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public override void OnResultExecuting(ResultExecutingContext context)
        {
            //var auditService = context.HttpContext.RequestServices.GetRequiredService<IAuditService>();
            if (context.Result is ObjectResult)
            {
                GetObjectResult(context);
            }
            else if (context.Result is EmptyResult)
            {
                context.Result = new ObjectResult(new ApiResponseDto
                {
                    Code = StatusCodes.Status404NotFound,
                    Status = ApiStatus.Failed
                });
            }
            else if (context.Result is ContentResult)
            {
                context.Result = new ObjectResult(new ApiResponseDto<object>
                {
                    Code = StatusCodes.Status200OK,
                    Result = (context.Result as ContentResult).Content
                });
            }
            else if (context.Result is FileContentResult fileResult)
            {
                var fileNameBytes = Encoding.UTF8.GetBytes(fileResult.FileDownloadName);
                var fileName = HttpUtility.UrlEncode(fileNameBytes);
                context.Result = new FileContentResult(fileResult.FileContents, fileResult.ContentType)
                {
                    FileDownloadName = fileName
                };
            }
            else if (context.Result is FileStreamResult streamResult)
            {
                var fileNameBytes = Encoding.UTF8.GetBytes(streamResult.FileDownloadName);
                var fileName = HttpUtility.UrlEncode(fileNameBytes);
                context.Result = new FileStreamResult(streamResult.FileStream, streamResult.ContentType)
                {
                    FileDownloadName = fileName
                };
            }
            else
            {
                var resultObj = context.Result as ObjectResult;
                if (resultObj == null) throw new CustomException("Internal server error");
                context.Result = new ObjectResult(new ApiResponseDto<object>
                {
                    Code = resultObj.StatusCode.HasValue ? resultObj.StatusCode.Value : StatusCodes.Status200OK,
                    Result = resultObj.Value
                });
            }
        }

        public static void GetObjectResult(ResultExecutingContext context)
        {
            if (context.Result is OkObjectResult)
            {
                var result = (context.Result as ObjectResult)?.Value;
                context.Result = new ObjectResult(new ApiResponseDto<object>
                {
                    Code = StatusCodes.Status200OK,
                    Status = ApiStatus.Success,
                    Result = result,
                    Message = ""
                });
            }
            else if (context.Result is BadRequestObjectResult)
            {
                var error = context.ModelState.Select(m => string.Join("; ", m.Value.Errors.Select(a => a.ErrorMessage))).Distinct();
                var errorMsg = string.Join(" ", error);
                _logger.Error(errorMsg);
                context.Result = new ObjectResult(new ApiResponseDto<object>
                {
                    Code = StatusCodes.Status400BadRequest,
                    Status = ApiStatus.Failed,
                    Message = errorMsg
                });
            }
            else if (context.Result is UnauthorizedResult)
            {
                context.Result = new ObjectResult(new ApiResponseDto
                {
                    Code = StatusCodes.Status401Unauthorized,
                    Status = ApiStatus.Failed,
                    Message = "Unauthorized"
                });
            }
            else if (context.Result is ForbidResult)
            {
                context.Result = new ObjectResult(new ApiResponseDto
                {
                    Code = StatusCodes.Status403Forbidden,
                    Status = ApiStatus.Failed,
                    Message = "Access denied"
                });
            }
            else if (context.Result is NotFoundObjectResult || context.Result is NotFoundResult)
            {
                context.Result = new ObjectResult(new ApiResponseDto
                {
                    Code = StatusCodes.Status404NotFound,
                    Status = ApiStatus.Failed,
                });
            }
            else if (context.Result is CreatedAtActionResult || context.Result is CreatedAtRouteResult || context.Result is CreatedResult)
            {
                context.Result = new ObjectResult(new ApiResponseDto
                {
                    Code = StatusCodes.Status201Created,
                    Status = ApiStatus.Success,
                });
            }
            else if (context.Result is AcceptedAtActionResult || context.Result is AcceptedAtRouteResult || context.Result is AcceptedResult)
            {
                context.Result = new ObjectResult(new ApiResponseDto
                {
                    Code = StatusCodes.Status202Accepted,
                    Status = ApiStatus.Success,
                });
            }
            else if (context.Result is ConflictObjectResult)
            {
                context.Result = new ObjectResult(new ApiResponseDto
                {
                    Code = StatusCodes.Status409Conflict,
                    Status = ApiStatus.Failed,
                });
            }
            else if (context.Result is UnprocessableEntityObjectResult)
            {
                context.Result = new ObjectResult(new ApiResponseDto
                {
                    Code = StatusCodes.Status422UnprocessableEntity,
                    Status = ApiStatus.Failed,
                });
            }
            else
            {
                var objR = context.Result as ObjectResult;
                if (objR == null)
                {
                    throw new CustomException("Internal server error");
                }
                context.Result = new ObjectResult(objR.Value);
            }
        }

    }
    //public class ApiResult
    //{
    //    public object? Result { get; set; }
    //    public string Message { get; set; } = string.Empty;
    //}
}

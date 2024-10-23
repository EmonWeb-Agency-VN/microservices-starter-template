using Common.Domain.Enums;

namespace Common.SharedKernel.ApiResponse
{
    public class ApiResponseDto
    {
        public int Code { get; set; }
        public string Message { get; set; } = string.Empty;
        public ApiStatus Status { get; set; }
    }

    public class ApiResponseDto<T> : ApiResponseDto where T : class
    {
        public T? Result { get; set; }
    }
}

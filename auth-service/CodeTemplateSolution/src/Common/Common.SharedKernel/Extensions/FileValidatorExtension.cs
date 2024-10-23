using Microsoft.AspNetCore.Http;

namespace Common.SharedKernel.Extensions
{
    public static class FileValidatorExtension
    {
        public static bool IsFileExtensionAllowed(IFormFile file, string[] allowedExtensions)
        {
            var extension = Path.GetExtension(file.FileName);
            return allowedExtensions.Contains(extension.ToLower());
        }
        public static bool IsFileSizeWithinLimit(IFormFile file, long maxSizeInBytes)
        {
            return file.Length <= maxSizeInBytes;
        }
    }
}

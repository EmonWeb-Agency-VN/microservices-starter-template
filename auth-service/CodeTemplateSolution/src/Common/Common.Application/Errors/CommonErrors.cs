using Common.SharedKernel.Errors;

namespace Common.Application.Errors
{
    public static class CommonErrors
    {
        public static Error ContextIsNull => new("Common.ContextIsNull", "Current context is null");
        public static Error RequestIsNull => new("Common.RequestIsNull", "Current request is null");
        public static Error InvalidFilterModel => new("Common.InvalidFilterModel", "Invalid filter model.");
        public static Error NotContainSpecialCharacters => new("Common.NotContainSpecialCharacters", "Value cannot contain special character.");
        public static Error NotMatchVietnameseName => new("Common.NotMatchVietnameseName", "Name is not Vietnamese name.");
    }

}

namespace Common.SharedKernel.Errors
{
    public class Error
    {
        public static readonly Error None = new(string.Empty, string.Empty);
        public static readonly Error UnhandledException = new("Error.UnhandledException", string.Empty);
        public static readonly Error NullOrEmptyValue = new("Error.NullOrEmptyValue", "The specified result value is null or empty.");
        public static readonly Error ConditionNotMet = new("Error.ConditionNotMet", "The specified condition was not met.");
        public static readonly Error ModelValidationError = new("Error.ModelValidationError", "Failed to validate request model.");
        public static readonly Error TokenValidationError = new("Error.TokenValidationError", "Failed to validate token.");

        public string Code { get; }
        public string Message { get; }
        public Error(string code, string message)
        {
            Code = code;
            Message = message;
        }

        public static implicit operator string(Error error) => error.Code;

        public static bool operator ==(Error? a, Error? b)
        {
            if (a is null && b is null)
            {
                return true;
            }

            if (a is null || b is null)
            {
                return false;
            }

            return a.Equals(b);
        }

        public static bool operator !=(Error? a, Error? b) => !(a == b);

        public virtual bool Equals(Error? other)
        {
            if (other is null)
            {
                return false;
            }

            return Code == other.Code && Message == other.Message;
        }

        public override bool Equals(object? obj) => obj is Error error && Equals(error);

        public override int GetHashCode() => HashCode.Combine(Code, Message);

        public override string ToString() => Code;
    }
}

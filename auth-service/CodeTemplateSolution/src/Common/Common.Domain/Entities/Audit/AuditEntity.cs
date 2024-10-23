using Common.Domain.Enums;

namespace Common.Domain.Entities.Audit
{
    public sealed class AuditEntity
    {
        public Guid Id { get; set; }
        public string Username { get; set; }
        public string IpAddress { get; set; }
        public string UserAgent { get; set; } = string.Empty;
        public string Description { get; set; }
        public string EntityName { get; set; }
        public AuditAction AuditAction { get; set; } = AuditAction.None;
        public ApiStatus Status { get; set; } = ApiStatus.None;
        public string Error { get; set; } = string.Empty;
        public string ErrorCode { get; set; } = string.Empty;
        public DateTimeOffset Timestamp { get; set; }
        public string Method { get; set; }
        public string ApiEndpoint { get; set; }
        public string ObjectInfo { get; set; }
    }
}

using Common.Domain.Enums;
using Common.Domain.Filters;

namespace Common.Domain.Entities.Audit
{
    [Serializable]
    public class AuditModel(ApiStatus status)
    {
        public string UserName { get; set; } = string.Empty;
        private string _errorDetail;
        public string ErrorDetail
        {
            get
            {
                if (Status == ApiStatus.Success) return string.Empty;
                return _errorDetail;
            }
            set
            {
                if (Status == ApiStatus.Success && !string.IsNullOrEmpty(value)) throw new InvalidOperationException("");
                _errorDetail = value;
            }
        }
        public AuditAction AuditAction { get; set; }
        public ApiStatus Status { get; set; } = status;
        public string ErrorCode { get; set; } = string.Empty;
    }

    public class AuditFilterModel : FilterModel
    {
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
    }
}

using Common.Domain.Entities.Audit;

namespace Common.Application.Audit
{
    [AttributeUsage(AttributeTargets.Method, Inherited = true)]
    public class AuditAttribute : Attribute
    {
        public AuditAction AuditAction { get; }

        public AuditAttribute(AuditAction auditAction)
        {
            AuditAction = auditAction;
        }
    }
}

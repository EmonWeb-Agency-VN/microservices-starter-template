using Common.Domain.Entities.Audit;

namespace Common.Domain.Interfaces
{
    public interface IAuditService
    {
        void SetAuditObj(AuditModel auditModel);
    }
}

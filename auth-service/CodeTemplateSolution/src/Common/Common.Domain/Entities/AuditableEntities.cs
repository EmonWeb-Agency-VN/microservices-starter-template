using Common.Domain.Entities.Users;

namespace Common.Domain.Entities
{
    public class AuditableEntities
    {
        public long CreatedById { get; set; }
        public virtual UserEntity CreatedBy { get; set; }
        public long ModifiedById { get; set; }
        public virtual UserEntity ModifiedBy { get; set; }
        public DateTimeOffset CreatedTime { get; set; }
        public DateTimeOffset ModifiedTime { get; set; }
    }
}

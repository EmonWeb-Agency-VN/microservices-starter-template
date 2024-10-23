using Common.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Common.Persistence.EntityConfigurations
{
    public static class AuditableEntityConfiguration
    {
        public static void BuildAuditableEntity<T>(this EntityTypeBuilder<T> builder) where T : AuditableEntities
        {
            builder.Property(a => a.CreatedById).IsRequired();
            builder.HasOne(a => a.CreatedBy).WithMany().HasForeignKey(a => a.CreatedById).OnDelete(DeleteBehavior.Restrict);
            builder.Property(a => a.CreatedTime).IsRequired();
            builder.Property(a => a.ModifiedById).IsRequired();
            builder.HasOne(a => a.ModifiedBy).WithMany().HasForeignKey(a => a.ModifiedById).OnDelete(DeleteBehavior.Restrict);
            builder.Property(a => a.ModifiedTime).IsRequired();
        }
    }
}

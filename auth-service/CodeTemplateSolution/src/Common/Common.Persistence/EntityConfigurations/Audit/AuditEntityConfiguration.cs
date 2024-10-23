using Common.Domain.Entities.Audit;
using Common.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Common.Infrastucture.EntityConfigurations.Audit
{
    public class AuditEntityConfiguration : IEntityTypeConfiguration<AuditEntity>
    {
        public void Configure(EntityTypeBuilder<AuditEntity> builder)
        {
            builder.ToTable($"Audit");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedOnAdd();
            builder.Property(x => x.Username).IsRequired();
            builder.Property(x => x.IpAddress).IsRequired();
            builder.Property(x => x.UserAgent).IsRequired();
            builder.Property(x => x.Description).IsRequired();
            builder.Property(x => x.EntityName).IsRequired();
            builder.Property(x => x.AuditAction).IsRequired();
            builder.Property(x => x.Status).IsRequired().HasDefaultValue(ApiStatus.None);
            builder.Property(x => x.Method).IsRequired();
            builder.Property(x => x.Error).IsRequired();
            builder.Property(x => x.ErrorCode).IsRequired();
            builder.Property(x => x.Timestamp).IsRequired();
            builder.Property(x => x.ApiEndpoint).IsRequired();
            builder.Property(x => x.ObjectInfo).IsRequired();
            builder.HasIndex(a => new { a.AuditAction, a.Status });
        }
    }
}

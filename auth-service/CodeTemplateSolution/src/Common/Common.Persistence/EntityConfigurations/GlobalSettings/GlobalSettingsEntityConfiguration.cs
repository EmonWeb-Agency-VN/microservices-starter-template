using Common.Domain.Entities.GlobalSettings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Common.Persistence.EntityConfigurations.GlobalSettings
{
    public class GlobalSettingsEntityConfiguration : IEntityTypeConfiguration<GlobalSettingsEntity>
    {
        public void Configure(EntityTypeBuilder<GlobalSettingsEntity> builder)
        {
            builder.ToTable($"GlobalSettings");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedOnAdd();
            builder.Property(x => x.Type).IsRequired();
            builder.Property(x => x.Detail).IsRequired();
            builder.BuildAuditableEntity();
        }
    }
}

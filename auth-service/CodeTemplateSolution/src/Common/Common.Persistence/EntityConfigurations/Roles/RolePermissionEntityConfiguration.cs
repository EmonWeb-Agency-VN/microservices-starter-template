using Common.Domain.Entities.Roles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Common.Persistence.EntityConfigurations.Roles
{
    public class RolePermissionEntityConfiguration : IEntityTypeConfiguration<RolePermissionEntity>
    {
        public void Configure(EntityTypeBuilder<RolePermissionEntity> builder)
        {
            builder.ToTable($"RolePermission");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedOnAdd();
            builder.Property(x => x.RoleId).IsRequired();
            builder.HasOne(a => a.Role).WithMany(a => a.RolePermissions).HasForeignKey(a => a.RoleId);
            builder.Property(x => x.Permission).IsRequired();
        }
    }
}

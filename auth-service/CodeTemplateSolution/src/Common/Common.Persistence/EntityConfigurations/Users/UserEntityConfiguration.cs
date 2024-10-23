using Common.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Common.Persistence.EntityConfigurations.Users
{
    public class UserEntityConfiguration : IEntityTypeConfiguration<UserEntity>
    {
        public void Configure(EntityTypeBuilder<UserEntity> builder)
        {
            builder.ToTable($"User");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedOnAdd();
            builder.Property(x => x.DisplayName).HasMaxLength(50).IsRequired();
            builder.Property(x => x.Email).HasMaxLength(128);
            builder.Property(x => x.UserName).HasMaxLength(50).IsRequired();
            builder.Property(x => x.Mobile).HasMaxLength(20);
            builder.Property(x => x.Gender).IsRequired();
            builder.Property(x => x.DateOfBirth).IsRequired();
            builder.Property(x => x.Password).IsRequired();
            builder.Property(x => x.PasswordSalt).HasDefaultValue(Array.Empty<byte>()).IsRequired();
            builder.Property(x => x.UserType).IsRequired();
            builder.Property(x => x.UserStatus).IsRequired();
            builder.Property(x => x.CreatedOn).IsRequired();
            builder.Property(x => x.LoginType).IsRequired();
            builder.Property(x => x.IsDeleted).IsRequired().HasDefaultValue(false);
            builder.Property(x => x.NeedChangePassword).IsRequired().HasDefaultValue(false);
        }
    }
}

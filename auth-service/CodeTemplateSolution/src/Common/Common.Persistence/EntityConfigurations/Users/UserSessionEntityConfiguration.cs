using Common.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Common.Persistence.EntityConfigurations.Users
{
    public class UserSessionEntityConfiguration : IEntityTypeConfiguration<UserSessionEntity>
    {
        public void Configure(EntityTypeBuilder<UserSessionEntity> builder)
        {
            builder.ToTable("UserSession");
            builder.HasKey(a => a.Id);
            builder.Property(a => a.Id);
            builder.Property(a => a.UserId);
            builder.Property(a => a.Mobile).HasMaxLength(128);
            builder.Property(a => a.AccessToken);
            builder.Property(a => a.RefreshToken);
            builder.Property(a => a.RefreshTokenExpireTime);
            builder.Property(a => a.LoginType);
            builder.Property(a => a.UserAgent);
            builder.Property(a => a.Roles);
            builder.Property(a => a.IpAddress);
            builder.Property(a => a.LastUpdateTime);
            builder.Property(a => a.LoginTime);
            builder.Property(a => a.LogoutTime).IsRequired(false);
            builder.Property(a => a.RememberLogin);
        }
    }
}

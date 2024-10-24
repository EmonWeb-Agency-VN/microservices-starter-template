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
            builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd().IsRequired();
            builder.Property(x => x.Email).HasColumnName("email").HasMaxLength(255).IsRequired();
            builder.Property(x => x.PhoneNumber).HasColumnName("phone_number").HasMaxLength(15).IsRequired(false);
            builder.Property(x => x.Password).HasColumnName("password").IsRequired();
            builder.Property(x => x.PasswordSalt).HasColumnName("password_salt").HasDefaultValue(Array.Empty<byte>()).IsRequired();
            builder.Property(x => x.IsEmailVerified).HasColumnName("is_email_verified").IsRequired().HasDefaultValue(false);
            builder.Property(x => x.IsPhoneVerified).HasColumnName("is_phone_verified").IsRequired().HasDefaultValue(false);
            builder.Property(x => x.OtpCode).HasColumnName("otp_code").HasMaxLength(6).HasDefaultValue(string.Empty);
            builder.Property(a => a.OtpExpiredAt).HasColumnName("otp_expires_at");
            builder.Property(a => a.FailedLoginAttempts).HasColumnName("failed_login_atempts").HasDefaultValue(0);
            builder.Property(a => a.AccoungLockedUntil).HasColumnName("account_locked_until");
            builder.Property(a => a.CreatedAt).HasColumnName("created_at").ValueGeneratedOnAdd();
            builder.Property(a => a.UpdatedAt).HasColumnName("updated_at").ValueGeneratedOnAddOrUpdate();
            builder.Property(a => a.LastLogin).HasColumnName("last_login");

            builder.HasIndex(a => new { a.Email, a.PhoneNumber }).HasDatabaseName("idx_email_phone");
            builder.HasIndex(a => a.Email).IsUnique();
            builder.HasIndex(a => a.PhoneNumber).IsUnique().HasFilter("[phone_number] IS NOT NULL");
        }
    }
}

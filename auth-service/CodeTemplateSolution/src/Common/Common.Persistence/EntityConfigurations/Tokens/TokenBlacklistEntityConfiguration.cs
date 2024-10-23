using Common.Domain.Entities.Tokens;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Common.Persistence.EntityConfigurations.Tokens
{
    public class TokenBlacklistEntityConfiguration : IEntityTypeConfiguration<TokenBlackListEntity>
    {
        public void Configure(EntityTypeBuilder<TokenBlackListEntity> builder)
        {
            builder.ToTable("TokenBlacklist");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedOnAdd();
            builder.Property(x => x.SessionId).IsRequired();
            builder.Property(x => x.AccessToken).IsRequired();
            builder.Property(x => x.RefreshToken).IsRequired();
            builder.BuildAuditableEntity();
        }
    }
}

using Common.SharedKernel.LogProvider;
using Microsoft.EntityFrameworkCore;

namespace Common.Persistence.Core
{
    public class UserDbContext : DbContext
    {
        public UserDbContext(DbContextOptions<UserDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            CommonModelBuilder.AppendConfiguration(modelBuilder);
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseLoggerFactory(LoggerHelper.GetLoggerFactory());
            base.OnConfiguring(optionsBuilder);
        }
    }
}

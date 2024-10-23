using Common.Persistence.Core;
using Common.Persistence.InitDataHelper;
using Common.SharedKernel.LogProvider;
using Microsoft.EntityFrameworkCore;
using NLog;
using System.Reflection;

namespace Auth.API.StartupTasks
{
    public sealed class MigrateDatabaseStartupTask(IHostEnvironment environment, IServiceProvider serviceProvider) : BackgroundService
    {
        private static readonly Logger logger = LoggerHelper.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                if (!environment.IsDevelopment())
                {
                    using IServiceScope scope2 = serviceProvider.CreateScope();
                    await InitDataAsync(scope2);
                    return;
                }
                using IServiceScope scope = serviceProvider.CreateScope();
                logger.Info("Start migrate database");
                await MigrateDatabaseAsync<UserDbContext>(scope, stoppingToken);
                logger.Info("Finish migrate database");
                await InitDataAsync(scope);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Failed to execute task");
                throw;
            }
        }

        private static async Task MigrateDatabaseAsync<TDbContext>(IServiceScope scope, CancellationToken cancellationToken)
            where TDbContext : DbContext
        {
            TDbContext dbContext = scope.ServiceProvider.GetRequiredService<TDbContext>();
            await dbContext.Database.MigrateAsync(cancellationToken);
        }

        private static async Task InitDataAsync(IServiceScope scope)
        {
            var dataService = scope.ServiceProvider.GetRequiredService<IDBDataService>();
            await dataService.InitDataAsync();
        }
    }
}



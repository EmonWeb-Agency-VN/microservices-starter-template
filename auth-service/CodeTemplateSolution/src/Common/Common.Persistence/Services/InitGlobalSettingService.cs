using System.Reflection;
using System.Transactions;
using Common.Domain.Entities.GlobalSettings;
using Common.Domain.Interfaces;
using Common.Persistence.InitDataHelper;
using Common.SharedKernel.LogProvider;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NLog;

namespace Common.Persistence.Services
{
    public class InitGlobalSettingService : DataInitService, IDataInitService
    {
        private static readonly Logger logger = LoggerHelper.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IDBRepository dBRepository;

        public int Step => 5;

        public string FileName => "";
        public InitGlobalSettingService(IDBRepository dBRepository)
        {
            this.dBRepository = dBRepository;
        }

        protected async override Task<ProcessStatus> InitSync()
        {
            var result = ProcessStatus.Pass;
            using TransactionScope scope = new(TransactionScopeAsyncFlowOption.Enabled);
            try
            {
                var existedData = await dBRepository.Context.Set<GlobalSettingsEntity>().ToListAsync();
                int deleteCount = dBRepository.DeleteRange(existedData);
                await dBRepository.SaveChangesAsync();
                logger.Info($"Row deleted: {deleteCount}");
                List<GlobalSettingsEntity> newGlobalSettings = new()
                {
                    new GlobalSettingsEntity
                    {
                        Type = GlobalType.AuthenticationSetting,
                        Detail = JsonConvert.SerializeObject(new AuthenticationSettings
                        {
                            DefaultSessionExpireTime = 60
                        })
                    },
                    new GlobalSettingsEntity
                    {
                        Type = GlobalType.BeforeTimeout,
                        Detail = JsonConvert.SerializeObject(new BeforeTimeoutSettings
                        {
                            BeforeTimeout = 5
                        })
                    },
                };
                await dBRepository.AddRangeAsync(newGlobalSettings);
                await dBRepository.SaveChangesAsync();
                scope.Complete();
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Init global settings failed. Message: {ex.Message}");
                scope.Dispose();
                return ProcessStatus.Failed;
            }
            return result;
        }

        public async Task<ProcessStatus> ExecuteAsync()
        {
            return await BaseExecute();
        }
    }
}

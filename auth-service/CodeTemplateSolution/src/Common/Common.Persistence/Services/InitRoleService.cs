using System.Reflection;
using System.Transactions;
using Common.Domain.Entities.Roles;
using Common.Domain.Interfaces;
using Common.Persistence.InitDataHelper;
using Common.SharedKernel;
using Common.SharedKernel.LogProvider;
using Microsoft.EntityFrameworkCore;
using NLog;

namespace Common.Persistence.Services
{
    public class InitRoleService(IDBRepository dBRepository) : DataInitService, IDataInitService
    {
        private static readonly Logger logger = LoggerHelper.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public int Step => 1;

        public string FileName => "";

        public async Task<ProcessStatus> ExecuteAsync()
        {
            return await base.BaseExecute();
        }

        protected async override Task<ProcessStatus> InitSync()
        {
            var result = ProcessStatus.Pass;
            using TransactionScope scope = new(TransactionScopeAsyncFlowOption.Enabled);
            try
            {
                var hasData = await dBRepository.Context.Set<RoleEntity>().AnyAsync();
                if (hasData)
                {
                    NeedInit = false;
                    logger.Info($"Roles already existed. Skip step {Step}.");
                    return result;
                }
                await dBRepository.AddAsync(Constants.AdminRole);
                await dBRepository.SaveChangesAsync();
                scope.Complete();
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Init role failed. MessagE: {ex.Message}");
                scope.Dispose();
                return ProcessStatus.Failed;
            }
            return result;
        }
    }
}

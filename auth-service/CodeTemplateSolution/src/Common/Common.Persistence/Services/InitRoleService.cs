using System.Reflection;
using System.Transactions;
using Common.Domain.Entities.Roles;
using Common.Domain.Interfaces;
using Common.Persistence.InitDataHelper;
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
                var listRoles = new List<RoleEntity>
                {
                    new ()
                    {
                        Id = RoleConstants.AdminRoleId,
                        Name = "",
                        Description = "",
                        RoleClassification = RoleClassification.BuiltIn,
                        RoleLevel = RoleLevel.System
                    },
                    new ()
                    {
                        Id = RoleConstants.UserRoleId,
                        Name = "",
                        Description = "",
                        RoleClassification = RoleClassification.BuiltIn,
                        RoleLevel = RoleLevel.Internal
                    },
                };
                await dBRepository.AddRangeAsync(listRoles);
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

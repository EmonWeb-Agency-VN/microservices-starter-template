using System.Reflection;
using Common.Domain.Entities.Roles;
using Common.Domain.Interfaces;
using Common.Persistence.InitDataHelper;
using Common.SharedKernel.LogProvider;
using Microsoft.EntityFrameworkCore;
using NLog;

namespace Common.Persistence.Services
{
    public class InitRolePermissionService(IDBRepository dBRepository) : DataInitService, IDataInitService
    {
        private static readonly Logger logger = LoggerHelper.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public int Step => 2;

        public string FileName => "";

        public async Task<ProcessStatus> ExecuteAsync()
        {
            return await base.BaseExecute();
        }

        protected async override Task<ProcessStatus> InitSync()
        {
            var result = ProcessStatus.Pass;
            try
            {
                var existData = await dBRepository.Context.Set<RolePermissionEntity>().ToListAsync();
                if (existData.Count != 0)
                {
                    dBRepository.DeleteRange(existData);
                    await dBRepository.SaveChangesAsync();
                }
                var rolePermissions = new List<RolePermissionEntity>();
                foreach (var rolePermission in RoleConstants.RolePermissionMappings)
                {
                    foreach (var permission in rolePermission.Permission)
                    {
                        var item = new RolePermissionEntity
                        {
                            RoleId = rolePermission.RoleId,
                            Permission = permission
                        };
                        rolePermissions.Add(item);
                    }
                }
                await dBRepository.AddRangeAsync(rolePermissions);
                await dBRepository.SaveChangesAsync();

            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Init role permission failed. Message: {ex.Message}");
                return ProcessStatus.Failed;
            }
            return result;
        }
    }
}

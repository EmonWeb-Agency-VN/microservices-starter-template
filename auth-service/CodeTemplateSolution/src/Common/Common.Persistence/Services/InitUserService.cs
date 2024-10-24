using Common.Domain.Entities.Roles;
using Common.Domain.Entities.Users;
using Common.Domain.Interfaces;
using Common.Persistence.InitDataHelper;
using Common.SharedKernel;
using Common.SharedKernel.LogProvider;
using Common.SharedKernel.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NLog;
using System.Reflection;

namespace Common.Persistence.Services
{
    public class InitUserService(IDBRepository dBRepository, IConfiguration configuration) : DataInitService, IDataInitService
    {
        private static readonly Logger logger = LoggerHelper.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public int Step => 2;
        public string FileName => "User.json";

        protected async override Task<ProcessStatus> InitSync()
        {
            var result = ProcessStatus.Pass;
            try
            {
                var roles = await dBRepository.Context.Set<RoleEntity>().ToListAsync();
                if (roles.Count == 0)
                {
                    logger.Error($"Roles have not been initialized yet.");
                    return ProcessStatus.Failed;
                }

                var existedUsers = await dBRepository.Context.Set<UserEntity>().ToListAsync();
                if (existedUsers.Count > 0)
                {
                    NeedInit = false;
                    logger.Info($"Data already existed. Skip step: {Step}");
                    return ProcessStatus.Pass;
                }
                var users = new List<UserEntity>();
                var password = PasswordUtils.HashPassword("i,M~FPzTL%0Z;1!f=T8O", out var userSalt);
                users.Add(Constants.SystemAdmin);
                await dBRepository.AddRangeAsync(users);
                var changedRecordNumber = await dBRepository.SaveChangesAsync();
                logger.Info($"Step: {Step} - Record created: {changedRecordNumber}.");
                var hasUserRoles = await dBRepository.Context.Set<UserRoleEntity>().AnyAsync();
                if (!hasUserRoles)
                {
                    await dBRepository.AddAsync(new UserRoleEntity()
                    {
                        UserId = Constants.SystemAdminId,
                        RoleId = Constants.AdminRoleId,
                    });
                    await dBRepository.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Init user failed. Message: {ex.Message}");
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

using Common.Domain.Entities.Roles;
using Common.Domain.Entities.Users;
using Common.Domain.Interfaces;
using Common.Persistence.InitDataHelper;
using Common.Persistence.SeedData.Models;
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
        public int Step => 3;
        public string FileName => "User.json";

        protected async override Task<ProcessStatus> InitSync()
        {
            var result = ProcessStatus.Pass;
            try
            {
                //var filePath = Path.Combine(SeedFolder, FileName);
                //if (!File.Exists(filePath))
                //{
                //    logger.Error($"File not found. File name: {FileName}");
                //    return ProcessStatus.Failed;
                //}
                //using StreamReader sr = new StreamReader(filePath);
                //var jsonUsers = JsonConvert.DeserializeObject<List<UserSeed>>(sr.ReadToEnd());
                var jsonUsers = new List<UserSeed>();

                //if (jsonUsers is null || jsonUsers.Count == 0)
                //{
                //    logger.Error($"File contains no data. File name: {FileName}");
                //    return ProcessStatus.Failed;
                //}
                jsonUsers.Add(new UserSeed
                {
                    Id = new Guid("c86d9bc1-7d17-4008-a5ab-ba5e5c9810c2"),
                    DisplayName = "System Admin",
                    Email = "",
                    UserName = "systemadmin",
                    Code = "SYS00",
                    Mobile = "",
                    Photo = "",
                    DateOfBirth = "",
                    Address = "",
                    Gender = Gender.Male,
                    UserType = UserType.Internal,
                    RoleType = RoleType.Admin,
                    LoginType = LoginType.Email
                });
                jsonUsers.Add(new UserSeed
                {
                    Id = new Guid("469bcc67-f389-4ab9-bd4c-aee05651a2c8"),
                    DisplayName = "Super Admin",
                    Email = "",
                    UserName = "user",
                    Code = "SA00",
                    Mobile = "",
                    Photo = "",
                    DateOfBirth = "",
                    Address = "",
                    Gender = Gender.Female,
                    UserType = UserType.Internal,
                    RoleType = RoleType.User,
                    LoginType = LoginType.Mobile | LoginType.Email
                });
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
                foreach (var jsonUser in jsonUsers)
                {
                    if (jsonUser.LoginType == LoginType.None
                        || jsonUser.RoleType == RoleType.None
                        || jsonUser.UserType == UserType.None
                        || jsonUser.Gender == Gender.None)
                    {
                        continue;
                    }
                    var existUser = existedUsers.FirstOrDefault(a => a.Id == jsonUser.Id);
                    if (existUser is not null)
                    {
                        existUser.DisplayName = jsonUser.DisplayName;
                        existUser.Gender = jsonUser.Gender;
                        existUser.Email = jsonUser.Email;
                        existUser.Mobile = jsonUser.Mobile;
                        existUser.UserType = jsonUser.UserType;
                        existUser.RoleType = jsonUser.RoleType;
                        existUser.LoginType = jsonUser.LoginType;
                    }
                    else
                    {
                        var user = new UserEntity
                        {
                            Id = jsonUser.Id,
                            DisplayName = jsonUser.DisplayName,
                            UserName = jsonUser.UserName,
                            Email = jsonUser.Email,
                            Mobile = jsonUser.Mobile,
                            Gender = jsonUser.Gender,
                            Password = password,
                            PasswordSalt = userSalt,
                            UserType = jsonUser.UserType,
                            RoleType = jsonUser.RoleType,
                            CreatedOn = DateTimeOffset.UtcNow,
                            LoginType = jsonUser.LoginType,
                            UserStatus = UserStatus.Active
                        };
                        users.Add(user);
                    }
                }
                await dBRepository.AddRangeAsync(users);
                var changedRecordNumber = await dBRepository.SaveChangesAsync();
                logger.Info($"Step: {Step} - Record created: {changedRecordNumber}.");
                var userRoles = new List<UserRoleEntity>();
                var hasUserRoles = await dBRepository.Context.Set<UserRoleEntity>().AnyAsync();
                if (!hasUserRoles)
                {
                    foreach (var jsonUser in jsonUsers)
                    {
                        Guid roleId;
                        if (jsonUser.RoleType == RoleType.Admin)
                        {
                            roleId = RoleConstants.AdminRoleId;
                        }
                        else if (jsonUser.RoleType == RoleType.User)
                        {
                            roleId = RoleConstants.UserRoleId;
                        }
                        else
                        {
                            roleId = RoleConstants.RolePermissionMappings.Find(a => a.RoleType == jsonUser.RoleType)!.RoleId;
                        }

                        userRoles.Add(new UserRoleEntity
                        {
                            Id = Guid.NewGuid(),
                            UserId = jsonUser.Id,
                            RoleId = roleId,
                            //ScopeId = roleId != RoleConstants.SuperAdminRoleId && roleId != RoleConstants.SystemAdminRoleId ? null : Guid.Empty,
                            //ScopeType = UserRolePermissionUtils.GetScopTypeFromRoleType(jsonUser.RoleType)
                        });
                    }
                    await dBRepository.AddRangeAsync(userRoles);
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

using Common.Domain.Entities.Users;
using Common.Domain.Interfaces;

namespace Common.Application.Implementations
{
    public class UserService(IDBRepository db) : IUserService
    {
        public async Task BatchAsignScopeMappings()
        {
            throw new NotImplementedException();
        }

        public async Task AssignUserRoles(List<long> roleIds, long newUserId)
        {
            var listUserRoles = new List<UserRoleEntity>();
            foreach (var id in roleIds)
            {
                var userRole = new UserRoleEntity
                {
                    UserId = newUserId,
                    RoleId = id,
                };
                listUserRoles.Add(userRole);
            }
            await db.AddRangeAsync(listUserRoles);
            await db.SaveChangesAsync();
        }
    }
}

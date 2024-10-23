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

        public async Task AssignUserRoles(List<Guid> roleIds, Guid newUserId, List<Guid>? resourceIds = null, ResourceType? resourceType = null)
        {
            var listUserRoles = new List<UserRoleEntity>();
            foreach (var id in roleIds)
            {
                var userRole = new UserRoleEntity
                {
                    Id = Guid.NewGuid(),
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

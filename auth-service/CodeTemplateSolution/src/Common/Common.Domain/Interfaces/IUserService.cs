using Common.Domain.Entities.Users;

namespace Common.Domain.Interfaces
{
    public interface IUserService
    {
        Task AssignUserRoles(List<Guid> roleIds, Guid newUserId, List<Guid>? resourceIds = null, ResourceType? resourceType = null);

        Task BatchAsignScopeMappings();
    }
}

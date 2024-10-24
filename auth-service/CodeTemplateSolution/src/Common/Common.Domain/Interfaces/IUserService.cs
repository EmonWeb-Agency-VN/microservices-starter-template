namespace Common.Domain.Interfaces
{
    public interface IUserService
    {
        Task AssignUserRoles(List<long> roleIds, long newUserId);

        Task BatchAsignScopeMappings();
    }
}

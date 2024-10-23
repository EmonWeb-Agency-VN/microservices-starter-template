namespace Common.Domain.Entities.Roles
{
    [AttributeUsage(AttributeTargets.All)]
    public class PermissionAttribute : Attribute
    {
        public PermissionType PermissionType { get; set; }
        public string PermissionName { get; set; }
        public int Value { get; set; }
        public PermissionDefinition ParentPermission { get; set; }

        public PermissionAttribute(PermissionType permissionType, string permissionName)
        {
            PermissionType = permissionType;
            PermissionName = permissionName;
        }
    }

    public enum PermissionType
    {
        None = 0,
        Administration = 15
    }

    //public static class PermissionUtil
    //{
    //    public static PermissionAttribute GetMetadata(this PermissionDefinition source)
    //    {

    //    }
    //}
}

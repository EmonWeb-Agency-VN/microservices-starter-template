namespace Common.Application.Audit
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class AuditIgnoreAttribute : Attribute
    {
    }
}

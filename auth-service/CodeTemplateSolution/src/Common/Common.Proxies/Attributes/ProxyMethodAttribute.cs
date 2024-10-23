namespace Common.Proxies.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class ProxyMethodAttribute : Attribute
    {
        public ProxyMethodAttribute(string method, string action)
        {
            Method = method;
            Action = action;
        }

        public string Method { get; }
        public string Action { get; }
    }
}

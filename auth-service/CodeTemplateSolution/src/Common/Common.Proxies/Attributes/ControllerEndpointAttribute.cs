namespace Common.Proxies.Attributes
{
    [AttributeUsage(AttributeTargets.Interface)]
    public class ControllerEndpointAttribute : Attribute
    {
        public ControllerEndpointAttribute(string controller)
        {
            Controller = controller;
        }

        public string Controller { get; }
    }
}

using Auth.API.Controllers;
using Auth.API.CustomConverter;
using Common.Proxies.CustomConverter;
using Microsoft.Extensions.DependencyInjection;

namespace Common.Proxies.Extensions
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddSharedInfrastructure(this IServiceCollection service)
        {
            service.AddControllers()
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.Converters.Add(new ClaimsIdentityConverter());
                    options.SerializerSettings.Converters.Add(new ClaimsPrincipalConverter());
                })
                .ConfigureApplicationPartManager(manager =>
                {
                    //manager.ApplicationParts.Clear();
                    //manager.ApplicationParts.Add();
                    manager.FeatureProviders.Add(new InternalControllerFeatureProvider());
                });
            return service;
        }
    }
}

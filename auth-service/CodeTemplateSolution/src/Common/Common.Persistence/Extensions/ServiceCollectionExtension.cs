using System.Reflection;
using Common.Persistence.InitDataHelper;
using Microsoft.Extensions.DependencyInjection;

namespace Common.Persistence.ServiceInstallers
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddScopedAsMatchingInterfaces(this IServiceCollection services, Assembly assembly)
        {
            var scopedTypes = assembly.GetTypes()
                .Where(t => typeof(IDataInitService).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

            foreach (var scopedType in scopedTypes)
            {
                services.AddScoped(scopedType);
            }

            return services;
        }
    }
}

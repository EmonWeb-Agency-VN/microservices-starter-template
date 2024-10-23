using System.Reflection;
using Common.SharedKernel.LogProvider;
using Microsoft.Extensions.DependencyInjection;
using NLog;

namespace Common.Persistence.InitDataHelper
{
    public class DataInitTask
    {
        private static readonly Logger logger = LoggerHelper.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public DataInitTask()
        {

        }
        public async Task<bool> RunAsync(IServiceProvider serviceProvider)
        {
            using (var scope = serviceProvider.CreateAsyncScope())
            {
                var services = scope.ServiceProvider;
                var allServices = typeof(IDataInitService).Assembly.GetTypes()
                                    .Where(t => t.IsClass && typeof(IDataInitService).IsAssignableFrom(t))
                                    .Select(a => (IDataInitService)services.GetService(a)).OrderBy(t => t.Step).ToList();
                foreach (var initService in allServices)
                {
                    var fileName = string.IsNullOrEmpty(initService.FileName) ? "(no files)" : initService.FileName;
                    logger.Info($"Begin [Step: {initService.Step} - File name: {fileName}]");
                    var status = await initService.ExecuteAsync();
                    if (status == ProcessStatus.Failed)
                    {
                        logger.Error($"Error [Step: {initService.Step} - File name: {fileName}]");
                        return false;
                    }
                    logger.Info($"End [Step: {initService.Step} - File name: {fileName}]");
                }
            }
            return true;
        }
    }
}

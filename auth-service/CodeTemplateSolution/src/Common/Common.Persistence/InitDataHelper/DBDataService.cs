using System.Reflection;
using Common.SharedKernel.LogProvider;
using NLog;

namespace Common.Persistence.InitDataHelper
{
    public class DBDataService(IServiceProvider serviceProvider) : IDBDataService
    {
        private static readonly Logger logger = LoggerHelper.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public async Task InitDataAsync()
        {
            logger.Info("Begin init data");
            var isSuccess = await new DataInitTask().RunAsync(serviceProvider);
            if (!isSuccess)
            {
                logger.Error("Failed to init data");
                throw new Exception();
            }
            logger.Info("Finish init data");
        }
    }
}

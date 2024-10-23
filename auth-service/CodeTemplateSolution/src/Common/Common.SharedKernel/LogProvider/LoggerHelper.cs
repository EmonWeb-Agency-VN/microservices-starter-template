using Microsoft.Extensions.Logging;
using NLog;
using NLog.Extensions.Logging;

namespace Common.SharedKernel.LogProvider
{
    public static class LoggerHelper
    {
        public static Logger GetLogger<T>() where T : class
        {
            var logger = LogManager.GetLogger(typeof(T).FullName);
            return logger;
        }

        public static Logger GetLogger(Type type)
        {
            var logger = LogManager.GetLogger(type.FullName);
            return logger;
        }

        public static ILoggerFactory GetLoggerFactory()
        {
            return LoggerFactory.Create(builder => { builder.AddNLog(); });
        }

        public static Microsoft.Extensions.Logging.ILogger CreateLogger(string type)
        {
            return GetLoggerFactory().CreateLogger(type);
        }
    }
}

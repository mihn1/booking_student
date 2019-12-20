using B2S.Core.Interfaces;
using NLog;
using System;

namespace B2S.Infrastructure.Utilities
{
    public class NLogger : INLogger
    {
        public void LogInfo(string message)
        {
            LogFactory factory = new LogFactory();
            Logger logger = factory.GetCurrentClassLogger();
            logger.Log(LogLevel.Info, message);
        }

        public void LogTrace(string message)
        {
            LogFactory factory = new LogFactory();
            Logger logger = factory.GetCurrentClassLogger();
            logger.Log(LogLevel.Trace, message);
        }

        public void LogError(Exception ex)
        {
            LogFactory factory = new LogFactory();
            Logger logger = factory.GetCurrentClassLogger();
            logger.Log(LogLevel.Error, ex.Message, ex);
        }
    }
}

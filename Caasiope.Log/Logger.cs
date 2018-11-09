using System;
using NLog;
using ILogger = Helios.Common.Logs.ILogger;

namespace Caasiope.Log
{
    public class LoggerAdapter : ILogger
    {
        public readonly Logger Logger;

        public LoggerAdapter(string name)
        {
            Logger = new Logger(name);
        }

        public void Log(string message, Exception exception = null)
        {
            Logger.LogError(message, exception);
        }

        public void LogInfo(string message, Exception exception = null)
        {
            Logger.LogInfo(message, exception);
        }

        public void LogDebug(string message, Exception exception = null)
        {
            Logger.LogDebug(message, exception);
        }
    }

    public class Logger
    {
        readonly NLog.Logger instance;

        public Logger(string target)
        {
            try
            {
                instance = LogManager.GetLogger(target);
            }
            catch
            {
                // ignored
            }

            if (instance == null)
                instance = LogManager.GetCurrentClassLogger();
        }

        public void LogInfo(string message, Exception exception = null)
        {
            try
            {
                if (exception != null)
                    instance.Info(exception, message);
                else
                    instance.Info(message);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception in NLog: {e}");
            }
        }

        public void LogError(string message, Exception exception = null)
        {
            try
            {
                if (exception != null)
                    instance.Error(exception, message);
                else
                    instance.Error(message);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception in NLog: {e}");
            }
        }

        public void LogDebug(string message, Exception exception = null)
        {
            try
            {
                if (exception != null)
                    instance.Debug(exception, message);
                else
                    instance.Debug(message);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception in NLog: {e}");
            }
        }
    }
}

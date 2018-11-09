using System;

namespace Helios.Common.Logs
{
    public interface ILogger
    {
        void Log(string message, Exception exception = null);
        void LogInfo(string message, Exception exception = null);
        void LogDebug(string message, Exception exception = null);
    }
}
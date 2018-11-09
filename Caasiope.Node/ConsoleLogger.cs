using System;
using Helios.Common.Logs;

namespace Caasiope.Node
{
	public class ConsoleLogger : ILogger
    {
        public void Log(string message, Exception exception = null)
        {
            Console.WriteLine(exception == null ? message : $"{message} {exception.Message}");
        }

        public void LogInfo(string message, Exception exception = null)
        {
            Console.WriteLine(exception == null ? message : $"{message} {exception.Message}");
        }

        public void LogDebug(string message, Exception exception = null)
        {
            Console.WriteLine(exception == null ? message : $"{message} {exception.Message}");
        }

        public static ConsoleLogger Instance = new ConsoleLogger();
    }
}

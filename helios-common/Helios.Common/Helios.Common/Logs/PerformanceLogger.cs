using System;
using System.Diagnostics;

namespace Helios.Common.Logs
{
    public class PerformanceLogger : IDisposable
    {
        private readonly ILogger logger;
        private readonly string name;

        private readonly Stopwatch watch = new Stopwatch();

        public PerformanceLogger(ILogger logger, string name)
        {
            this.logger = logger;
            this.name = name;
            watch.Start();
        }

        public void Dispose()
        {
            watch.Stop();
            var elapsed = watch.ElapsedMilliseconds;
            logger.Log($"Method {name} -> Elapsed Time {elapsed} ms", null);
        }
    }
}
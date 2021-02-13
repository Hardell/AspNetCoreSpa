using System;
using System.Diagnostics;

namespace AspNetCoreSpa.Infrastructure.OnlineUserManager
{
    public class StopWatchWithOffset
    {
        private readonly Stopwatch _stopwatch;
        private readonly TimeSpan _offsetTimeSpan;

        public StopWatchWithOffset(TimeSpan elapsedTimeSpan)
        {
            _offsetTimeSpan = elapsedTimeSpan;
            _stopwatch = new Stopwatch();
        }

        public void Start()
        {
            _stopwatch.Start();
        }

        public void Stop()
        {
            _stopwatch.Stop();
        }

        public TimeSpan ElapsedTimeSpan => _stopwatch.Elapsed + _offsetTimeSpan;
    }
}

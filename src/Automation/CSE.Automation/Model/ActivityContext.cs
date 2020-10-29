using CSE.Automation.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace CSE.Automation.Model
{
    public sealed class ActivityContext : IDisposable
    {
        public ActivityContext()
        {
            Timer = new Stopwatch();
            Timer.Start();
        }
        public ActivityContext(string activityName) : this()
        {
            ActivityName = activityName;
        }

        public void End()
        {
            if (Timer is null) return;
            _elapsed = Timer.Elapsed;
            Timer = null;
        }

        public string ActivityName { get; set; }
        public Guid ActivityId => Guid.NewGuid();
        public DateTimeOffset StartTime => DateTimeOffset.Now;

        private TimeSpan? _elapsed;
        private bool disposedValue;

        public TimeSpan ElapsedTime { get { return _elapsed ?? Timer.Elapsed; } }

        [JsonIgnore]
        public Stopwatch Timer { get; private set; }

        public ActivityContext WithLock(IDeltaProcessor processor)
        {
            processor.Lock();
            return this;
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~ActivityContext()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }


    }
}

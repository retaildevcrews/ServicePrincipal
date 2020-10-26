using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Newtonsoft.Json;

namespace CSE.Automation.Model
{
    public sealed class ActivityContext
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
        public TimeSpan ElapsedTime { get { return _elapsed ?? Timer.Elapsed; } }

        [JsonIgnore]
        public Stopwatch Timer { get; private set; }
    }
}

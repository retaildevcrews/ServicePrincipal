using System;

namespace CSE.Automation.Model
{
    public sealed class ActivityContext
    {
        public Guid ActivityId => Guid.NewGuid();
        public DateTimeOffset StartTime => DateTimeOffset.Now;
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace CSE.Automation.Model
{
    public sealed class ActivityContext
    {
        public Guid ActivityId => Guid.NewGuid();
        public DateTimeOffset StartTime => DateTimeOffset.Now;
    }
}

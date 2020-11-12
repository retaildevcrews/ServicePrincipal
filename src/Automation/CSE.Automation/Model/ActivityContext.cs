// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using CSE.Automation.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace CSE.Automation.Model
{
    internal sealed class ActivityContext : IDisposable
    {
        public ActivityContext(IActivityService activityService)
            : this()
        {
            this.activityService = activityService;
        }

        private ActivityContext()
        {
            Timer = new Stopwatch();
            Timer.Start();
        }

        public void End(ActivityHistoryStatus status = ActivityHistoryStatus.Completed)
        {
            if (status != ActivityHistoryStatus.Completed && status != ActivityHistoryStatus.Failed)
            {
                throw new ArgumentOutOfRangeException(nameof(status));
            }

            this.Activity.Status = ActivityHistoryStatus.Completed;

            if (Timer is null)
            {
                return;
            }

            elapsed = Timer.Elapsed;
            Timer = null;
        }

        public ActivityHistory Activity { get; set; }

        public DateTimeOffset StartTime { get; } = DateTimeOffset.Now;
        public string CorrelationId { get; private set; } = Guid.NewGuid().ToString();
        private IActivityService activityService;
        private IDeltaProcessor processor;
        private TimeSpan? elapsed;
        private bool disposedValue;
        private bool isLocked;

        public TimeSpan ElapsedTime { get { return elapsed ?? Timer.Elapsed; } }

        [JsonIgnore]
        public Stopwatch Timer { get; private set; }

        public ActivityContext WithProcessorLock(IDeltaProcessor deltaProcessor)
        {
            if (deltaProcessor == null)
            {
                throw new ArgumentNullException(nameof(deltaProcessor));
            }

            deltaProcessor.Lock().Wait();

            isLocked = true;
            processor = deltaProcessor;
            return this;
        }

        /// <summary>
        /// Set the correlation id of the activity in the context.
        /// </summary>
        /// <param name="correlationId">A correlationId to use for this context.</param>
        /// <returns>The instance of the ActivityHistroy</returns>
        public ActivityContext WithCorrelationId(string correlationId)
        {
            CorrelationId = correlationId;
            if (Activity != null)
            {
                Activity.CorrelationId = CorrelationId;
            }

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
                    if (activityService != null)
                    {
                        if (this.Activity.Status != ActivityHistoryStatus.Failed)
                        {
                            this.Activity.Status = ActivityHistoryStatus.Completed;
                        }

                        activityService.Put(this.Activity).Wait();
                    }

                    if (isLocked)
                    {
                        processor.Unlock().Wait();
                        isLocked = false;
                    }
                }

                disposedValue = true;
            }
        }
    }
}

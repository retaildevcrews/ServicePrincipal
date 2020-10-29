using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

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
        public string LockProcessor(Container container)
        {
            Console.WriteLine("Attempting to lock processor");
            try
            {
                // If read fails that means this is the first run, therefore a lock needs to be set on a default config
                var readResponse = container.ReadItemAsync<ProcessorConfiguration>("02a54ac9-441e-43f1-88ee-fde420db2559", new PartitionKey("ServicePrincipal")).Result;
                Console.WriteLine("Previous Run Exists with Etag: " + readResponse.ETag);
                ItemRequestOptions requestOptions = new ItemRequestOptions { IfMatchEtag = readResponse.ETag };
                ProcessorConfiguration config = readResponse.Resource;
                if (config.IsProcessorLocked)
                {
                    Console.WriteLine("Locking Failed");
                    throw new System.Exception("Processor previously locked, try again later");
                }
                config.IsProcessorLocked = true;
                // If replace fails that means config was locked by external source in race condition
                var updateResponse = container.ReplaceItemAsync<ProcessorConfiguration>(config, config.Id, new PartitionKey("ServicePrincipal"), requestOptions);
                Console.WriteLine("Locking Successful With Etag: " + updateResponse.Result.ETag);
                return updateResponse.Result.ETag;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.GetType().Name);
                Console.WriteLine(e.Message);
                var config = new ProcessorConfiguration
                {
                    ConfigType = ProcessorType.ServicePrincipal,
                    Id = "02a54ac9-441e-43f1-88ee-fde420db2559",
                    FilterString = "filterstring",
                    SelectFields = new List<string> { "appId",
                        "displayName",
                        "notes",
                        "owners",
                        "notificationEmailAddresses",
                    },
                    DeltaLink = "",
                    RunState = RunState.SeedAndRun,
                    Name = "ServicePrincipal Processor",
                    Description = "Descriptive Text",
                    IsProcessorLocked = true,
                };
                // If create fails that means configs was locked by external source in race condition
                var response = container.CreateItemAsync<ProcessorConfiguration>(config, new PartitionKey("ServicePrincipal"));
                Console.WriteLine("Locking Successful With Etag: " + response.Result.ETag);
                return response.Result.ETag;
            }
        }
        public string UnlockProcessor(Container container)
        {
            Console.WriteLine("Attempting to unlock processor");
            var config = container.ReadItemAsync<ProcessorConfiguration>("02a54ac9-441e-43f1-88ee-fde420db2559", new PartitionKey("ServicePrincipal")).Result.Resource;
            config.IsProcessorLocked = false;
            var response = container.ReplaceItemAsync<ProcessorConfiguration>(config, config.Id, new PartitionKey("ServicePrincipal"));
            return response.Result.ETag;
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

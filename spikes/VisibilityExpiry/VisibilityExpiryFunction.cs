using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Storage.Queue;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;

namespace VisibilityExpiry
{
    public static class VisibilityExpiryFunction
    {
        [FunctionName("VisibilityExpiry")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            [Queue("outqueue"), StorageAccount("AzureWebJobsStorage")] CloudQueue msg, ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string name = req.Query["name"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            name = name ?? data?.name;

            string responseMessage = string.IsNullOrEmpty(name)
                ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
                : $"Hello, {name}. This HTTP triggered function executed successfully.";

            if (!string.IsNullOrEmpty(name))
            {
                // Add a message to the output collection.
                //msg.Add(string.Format("Name passed to the function: {0}", name));
                await msg.AddMessageAsync(new CloudQueueMessage(JsonConvert.SerializeObject(responseMessage)), null, TimeSpan.FromSeconds(5), null, null);
            }

            log.LogInformation($"Added message: \"{responseMessage}\" to queue \n");

            return new OkObjectResult("You have started an unstoppable chain of events");
        }
    }
}

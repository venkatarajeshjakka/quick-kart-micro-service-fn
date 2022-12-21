using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Azure.Messaging.ServiceBus;

namespace QucikKartLogin
{
    public static class Payment
    {
        [FunctionName("Payment")]
        public static async Task<bool> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string name = req.Query["name"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            

            //Insert into Queue

            ServiceBusClient client;

            ServiceBusSender sender;

            var clientOptions = new ServiceBusClientOptions()
            {
                TransportType = ServiceBusTransportType.AmqpWebSockets
            };

            client = new ServiceBusClient("Endpoint=sb://sb-quickkart-group4.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=AjMcaDj2UMwd5DmP6MaElgXpP1spa8l+pHG6KsSJvnk=", clientOptions);
            sender = client.CreateSender("payment-queue");

            using ServiceBusMessageBatch messageBatch = await sender.CreateMessageBatchAsync();

            var responseMessage = false;

            // try adding a message to the batch
            if (!messageBatch.TryAddMessage(new ServiceBusMessage(requestBody)))
            {
                // if it is too large for the batch
                throw new Exception($"The message  is too large to fit in the batch.");
            }


            try
            {
                // Use the producer client to send the batch of messages to the Service Bus queue
                await sender.SendMessagesAsync(messageBatch);
                responseMessage = true;

            }
            catch (Exception ex)
            {

            }
            finally
            {
                // Calling DisposeAsync on client types is required to ensure that network
                // resources and other unmanaged objects are properly cleaned up.
                await sender.DisposeAsync();
                await client.DisposeAsync();
            }

            
            return responseMessage;
        }

       
    }
}

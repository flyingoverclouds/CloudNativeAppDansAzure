#define LOCAL

using Bus.Services.Helper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Repositories.Services.Interfaces;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace CnaFuncOnCatalog
{
    public static class FuncOnCatalog
    {
        private static IConfiguration _configuration;
        private static IConfiguration CreateConfiguration(ExecutionContext context)
        {
            var builder = new ConfigurationBuilder();
#if LOCAL

            builder
                .SetBasePath(context.FunctionAppDirectory)
                .AddUserSecrets("bc774958-108a-48a1-9549-8b824259e62a")
                .AddJsonFile("local.settings.json", true, reloadOnChange: true)
                .AddEnvironmentVariables();


#endif
            return builder.Build();
        }
        private static IQueueClient _queueClient;
        private static CnaServicesBus _cnaServiceBus;
        private static IQueueClient CreateOrGetServiceBusQueue(string connectionString, string queueName)
        {
            if (_cnaServiceBus==null)
            {
                _cnaServiceBus=new CnaServicesBus(connectionString);                
            }
            if (_queueClient==null)
            {
                _queueClient = _cnaServiceBus.CreateQueueClient(queueName);
            }

            return _queueClient;
        }
        
        private static Message CreateMessage(string body)
        {
            CatalogItem item = JsonConvert.DeserializeObject<CatalogItem>(body);
            item.Id = Guid.NewGuid().ToString();
            item.ProductId = Guid.NewGuid().ToString();
            
            var jsonBody = JsonConvert.SerializeObject(item);
            return new Message(Encoding.UTF8.GetBytes(jsonBody));

        }
        [FunctionName("AddItem")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log, ExecutionContext context)
        {
            
            //TODO:DEMO SEND MESSAGE
            log.LogInformation("C# HTTP trigger function processed a request.");            
            if (_configuration ==null)
            {
                _configuration = CreateConfiguration(context);
            }

            var QueueName = _configuration["QueueName"];
            var ServiceBusConnectionString = _configuration["ServiceBusConnectionString"];

            _queueClient = CreateOrGetServiceBusQueue(ServiceBusConnectionString, QueueName);
            
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            var message = CreateMessage(requestBody);
            
            // Envoi le message à la file d'attente
            await _queueClient.SendAsync(message);

            return new OkObjectResult("cool!!");
        }
        
    }

}

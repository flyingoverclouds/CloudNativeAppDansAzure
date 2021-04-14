using Microsoft.Azure.ServiceBus;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Bus.Services.Helper
{
    public class CnaServicesBus
    {
        private IQueueClient _queueClient;
        private IQueueClient _queueReceive;
        private ISubscriptionClient _subscriptionClient;
        private string _connectionString;
        private ServiceBusConnection _serviceBusConnection;
        public MessageHandler MessageHandler { get; set; }

        public ITopicClient _topicClient;
        public CnaServicesBus(string nameSpaceConnectionString)
        {
            MessageHandler = new MessageHandler();
            _connectionString = nameSpaceConnectionString;
            _serviceBusConnection = new ServiceBusConnection(_connectionString);

        }
        public IQueueClient RegisterReceiverToQueue(Func<Message, CancellationToken,Task> handler,string queueName)
        {

            _queueReceive = CreateQueueClient(queueName);
            MessageHandler.ReceiveQueue = _queueReceive;
            //TODO :put the number 10 in config filer
            var messageHandlerOptions = CreateMessageHandlerOptions(10);

            // Enregistrer la fonction qui traitera les messages
            _queueReceive.RegisterMessageHandler(handler, messageHandlerOptions);
            return _queueReceive;

        }
        public ISubscriptionClient AddReceiverToSubscriptionTopic(Func<Message, CancellationToken, Task> handler,string topicName, string subscriptionName)
        {
            var retryPolicy = CreateRetryExponential();
            _subscriptionClient =
                new Microsoft.Azure.ServiceBus.SubscriptionClient(_serviceBusConnection,
                                                                  topicName,
                                                                  subscriptionName,
                                                                  ReceiveMode.PeekLock,
                                                                  retryPolicy);


            //TODO put number 1 in config file
            var messageHandlerOptions = CreateMessageHandlerOptions(1);

            _subscriptionClient.RegisterMessageHandler(handler,
                                                       messageHandlerOptions);
            return _subscriptionClient;
        }

        public async Task RemoveReceiveQueueClientAsync()
        {
            if (_queueReceive == null)
            {
                throw new NullReferenceException(nameof(_queueReceive));
            }
            await _queueReceive.CloseAsync();
            _queueReceive = MessageHandler.ReceiveQueue = null;
        }
        private MessageHandlerOptions CreateMessageHandlerOptions(int maxConcurrentCall)
        {
            return new MessageHandlerOptions(MessageHandler.ExceptionReceivedHandler)
            {

                // Nombre maximum d'appels simultanés au callback `ProcessMessagesAsync`, fixé à 1 pour plus de simplicité.
                // Réglez-le en fonction du nombre de messages que l'application veut traiter en parallèle.
                MaxConcurrentCalls = 1,

                // Indique si MessagePump doit automatiquement compléter les messages après le retour du rappel de l'utilisateur.
                // Faux ci-dessous indique que le Complete sera traité par le User Callback comme dans `ProcessMessagesAsync` ci-dessous.
                AutoComplete = false
            };
        }
        public IQueueClient CreateQueueClient(string queueName)
        {
            
            //TODO: Put in settings in config file
            var retryPolicy = new RetryExponential(
                               minimumBackoff: TimeSpan.FromSeconds(10),
                               maximumBackoff: TimeSpan.FromSeconds(30),
                               maximumRetryCount: 3);

            return _queueClient = new QueueClient(_serviceBusConnection, 
                                                  queueName,
                                                  ReceiveMode.PeekLock,
                                                  retryPolicy);
                                                      
            
         
        }
        public ITopicClient CreateTopic(string topicName)
        {
            var retryPolicy = new RetryExponential(
                              minimumBackoff: TimeSpan.FromSeconds(10),
                              maximumBackoff: TimeSpan.FromSeconds(30),
                              maximumRetryCount: 3);
            return _topicClient = new TopicClient(_serviceBusConnection, topicName,retryPolicy);
        }
        public async Task SendMessageToQueueAsync(Message message)
        {
            if (_queueClient == null)
            {
                throw new NullReferenceException(nameof(_queueClient));
            }

            await _queueClient.SendAsync(message);
        }
        public Message CreateMessage(string message)
        {
            var msg = new Message(Encoding.UTF8.GetBytes(message));

            return msg;
        }

        public Message CreateMessage<T>(T item)
        {
            var jsonData = JsonConvert.SerializeObject(item);
            var message = new Message(Encoding.UTF8.GetBytes(jsonData));
            return message;
        }

        
        public  RetryExponential CreateRetryExponential()
        {
            return new RetryExponential(
                               minimumBackoff: TimeSpan.FromSeconds(10),
                               maximumBackoff: TimeSpan.FromSeconds(30),
                               maximumRetryCount: 3);
        }


    }
}

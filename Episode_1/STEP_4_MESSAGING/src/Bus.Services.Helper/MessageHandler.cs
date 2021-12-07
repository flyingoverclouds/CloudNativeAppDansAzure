using Microsoft.Azure.ServiceBus;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Bus.Services.Helper
{
    public class MessageHandler
    {
        public class MessageEventArgs : EventArgs
        {
            public string Message { get; set; }
        }
        public delegate void OnMessageHandler(object sender, MessageEventArgs args);
        public event OnMessageHandler OnMessage;
        public event OnMessageHandler OnError;
        private IQueueClient _receiveQueue = null;
        public  IQueueClient ReceiveQueue { get => _receiveQueue; set => _receiveQueue = value; }
        
        
        public async Task ProcessMessagesAsync(Message message, CancellationToken token)
        {

            // Process the message
            var MessageReceive = $"SN:{message.SystemProperties.SequenceNumber} : {Encoding.UTF8.GetString(message.Body)}";

            // Complete the message so that it is not received again.
            // This can be done only if the queueClient is created in ReceiveMode.PeekLock mode (which is default).
            if (ReceiveQueue != null) await ReceiveQueue.CompleteAsync(message.SystemProperties.LockToken);

            // Note: Use the cancellationToken passed as necessary to determine if the queueClient has already been closed.
            // If queueClient has already been Closed, you may chose to not call CompleteAsync() or AbandonAsync() etc. calls 
            // to avoid unnecessary exceptions.


            OnMessage?.Invoke(null, new MessageEventArgs { Message = MessageReceive });


        }
        
        public Task ExceptionReceivedHandler(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
        {

            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Message handler encountered an exception {exceptionReceivedEventArgs.Exception}.");
            var context = exceptionReceivedEventArgs.ExceptionReceivedContext;
            sb.AppendLine("Exception context for troubleshooting:");
            sb.AppendLine($"- Endpoint: {context.Endpoint}");
            sb.AppendLine($"- Entity Path: {context.EntityPath}");
            sb.AppendLine($"- Executing Action: {context.Action}");
            var ErrorMessage = sb.ToString();

            return Task.CompletedTask;
        }

    }
}

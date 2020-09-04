using Microsoft.Azure.ServiceBus;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace RTTCockpit
{
    public static class ServiceBusHandler
    {
        private static QueueClient _queueClient;
        
        public static event EventHandler<MessageReceivedEventArgs> MessageReceived;


        public static void RegisterQueue(this string queue)
        {
            var connectionString = ConfigurationManager.AppSettings["ServiceBusConnectionString"].ToString();
            _queueClient = new QueueClient(connectionString, queue);
            RegisterOnMessageHandlerAndReceiveMessages();
        }

        private static void RegisterOnMessageHandlerAndReceiveMessages()
        {
            var messageHandlerOptions = new MessageHandlerOptions(ExceptionReceivedHandler)
            {
                AutoComplete = true
            };

            _queueClient.RegisterMessageHandler(ProcessMessagesAsync, messageHandlerOptions);
        }

        private static async Task ProcessMessagesAsync(Message message, CancellationToken token)
        {
            MessageReceived?.Invoke("ServiceBusHandler", new MessageReceivedEventArgs(message));
        }

        private static Task ExceptionReceivedHandler(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
        {
            Console.WriteLine($"Message handler encountered an exception {exceptionReceivedEventArgs.Exception}.");
            var context = exceptionReceivedEventArgs.ExceptionReceivedContext;
            Console.WriteLine("Exception context for troubleshooting:");
            Console.WriteLine($"- Endpoint: {context.Endpoint}");
            Console.WriteLine($"- Entity Path: {context.EntityPath}");
            Console.WriteLine($"- Executing Action: {context.Action}");
            return Task.CompletedTask;
        }
    }
}

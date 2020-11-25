using Microsoft.Azure.ServiceBus;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Receiver
{   
    class Receiver
    {
        const string ServiceBusConnectionString = "Endpoint=sb://messagingcourse.servicebus.windows.net/;SharedAccessKeyName=course;SharedAccessKey=ramt/bWgACAYJq/S7zjMcjCTNGp6zxjWe3Q6hBot7CI=";
        const string QueueName = "javi";
        static IQueueClient queueClient;

        static async Task Main(string[] args)
        {
            queueClient = new QueueClient(ServiceBusConnectionString, QueueName);

            var messageHandlerOptions = new MessageHandlerOptions(ExceptionReceivedHandler)
            {
                // Maximum number of Concurrent calls to the callback `ProcessMessagesAsync`, set to 1 for simplicity.
                // Set it according to how many messages the application wants to process in parallel.
                MaxConcurrentCalls = 1
            };

            // Register the function that will process messages
            queueClient.RegisterMessageHandler(ProcessMessagesAsync, messageHandlerOptions);
            Console.ReadKey();
            // We need to close the consumer to stop receiving and inform Service Bus broker
            await queueClient.CloseAsync();
        }

        static Task ExceptionReceivedHandler(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
        {
            Console.WriteLine($"Message handler encountered an exception {exceptionReceivedEventArgs.Exception}.");
            return Task.CompletedTask;
        }

        static Task ProcessMessagesAsync(Message message, CancellationToken token)
        {            
            // Process the message
            Console.WriteLine($"Received message: SequenceNumber:{message.SystemProperties.SequenceNumber} Body:{Encoding.UTF8.GetString(message.Body)}");
            return Task.CompletedTask;
        }
    }
}

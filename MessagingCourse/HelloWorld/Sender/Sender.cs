using Microsoft.Azure.ServiceBus;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Messaging
{
    class Sender
    {
        const string ServiceBusConnectionString = "Endpoint=sb://messagingcourse.servicebus.windows.net/;SharedAccessKeyName=course;SharedAccessKey=ramt/bWgACAYJq/S7zjMcjCTNGp6zxjWe3Q6hBot7CI=";
        const string QueueName = "javi";
        static IQueueClient queueClient;

        static async Task Main(string[] args)
        {
            queueClient = new QueueClient(ServiceBusConnectionString, QueueName);
            try
            {
                string messageBody = $"{DateTime.Now}: Hello World!";
                var message = new Message(Encoding.UTF8.GetBytes(messageBody));

                // Write the body of the message to the console
                Console.WriteLine($"Sending message: {messageBody}");

                // Send the message to the queue
                await queueClient.SendAsync(message);

                Console.WriteLine($"Message has been sent");
            }
            catch (Exception exception)
            {
                Console.WriteLine($"{DateTime.Now} :: Exception: {exception.Message}");
            }
        }        
    }
}

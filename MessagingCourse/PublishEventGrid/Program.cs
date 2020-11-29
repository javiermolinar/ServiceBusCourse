using Microsoft.Azure.EventGrid;
using Microsoft.Azure.EventGrid.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventGridPublisher
{
    class Program
    {
        const string EventGridTopicApiKey = "";
        const string TopicHost = "";
        static async Task Main(string[] args)
        {
            IEventGridClient eventGridClient =  new EventGridClient(new TopicCredentials(EventGridTopicApiKey));
            var topic = new Uri(TopicHost).Host;

            var eGridEvent = new EventGridEvent()
            {
                Id = Guid.NewGuid().ToString(),
                EventTime = DateTime.UtcNow,
                DataVersion = "v1",
                EventType = "OrderCreated",
                Data = "Order by user Javi has been created",
                Subject = "ALL"
            };

            await eventGridClient.PublishEventsAsync(topic, new List<EventGridEvent>() { eGridEvent });

            Console.WriteLine("Event has been sent!");
        }
    }
}

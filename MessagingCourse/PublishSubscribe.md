## Publish Subscribe

In the previous tutorial we created a work queue. In a queue each task is delivered to **exactly** one worker. In this part we'll do something completely different -- we'll deliver a message to multiple consumers. This pattern is known as "publish/subscribe".

To illustrate the pattern, lets image logging system. It will consist of two programs -- the first will emit log messages and the second will receive and print them.

In our logging system every running copy of the receiver program will get the messages. That way we'll be able to run one receiver and direct the logs to disk; and at the same time we'll be able to run another receiver and see the logs on the screen.

Essentially, published log messages are going to be broadcast to all the receivers.

### Exchange

When using topics and subscriptions, components of a distributed application do not communicate directly with each other; instead they exchange messages via a topic, which acts as an intermediary.

In contrast with Service Bus queues, in which each message is processed by a single consumer, topics and subscriptions provide a one-to-many form of communication, using a publish/subscribe pattern. It is possible to register multiple subscriptions to a topic. When a message is sent to a topic, it is then made available to each subscription to handle/process independently.

<p align ="center">
<img src ="https://docs.microsoft.com/en-us/azure/service-bus-messaging/media/service-bus-java-how-to-use-topics-subscriptions/sb-topics-01.png" >
</p>

### How to publish/subscribe to a topic

This time we will use ITopicClient for sending

```cs
static ITopicClient topicClient;
topicClient = new TopicClient(ServiceBusConnectionString, TopicName);
await topicClient.SendAsync(message);
```

For receiving we need first to specify the name of the subscription that we want to connect. It has to be created previusly.

```cs
static ISubscriptionClient subscriptionClient;
subscriptionClient = new SubscriptionClient(ServiceBusConnectionString, TopicName, SubscriptionName);
```

And just as the queue client
```cs
subscriptionClient.RegisterMessageHandler(ProcessMessagesAsync, messageHandlerOptions);
```


### Let's create a chat client

For chatting you usually need first to enter on a chat room before been able to see  the messages.
Any user can send messages and see other people messages. A publish subscsribe pattern can help us.

We will modify our existing sender to connect to the channel and start sending messages:
This time we will use a TopicClient

```cs
const string ServiceBusConnectionString = "<connectionstring>";
const string TopicName = "<channel name>";       
// Topic client
static ITopicClient topicClient;

static async Task Main(string[] args)
{
    topicClient = new TopicClient(ServiceBusConnectionString, TopicName);
    Console.WriteLine("Write your message and press enter to send it to the channel");         

    while(true) {
        Console.Write(">");
        string message = Console.ReadLine();
        var sbMessage = new Message(Encoding.UTF8.GetBytes(message));
        // We will use the user properties bag where we can add any key value we want
        // This way we can mark a message to an user
        sbMessage.UserProperties.Add("User", "Javi");

        // Message is sent as before
        await topicClient.SendAsync(sbMessage);
    }                
}      
```

Our receiver client won't be quite different than before. This time we will connect to the topic as before but also for a specific subscription which
is no more than a sub-queue of the topic.

```cs
const string ServiceBusConnectionString = "<connectionstring>";
const string TopicName = "<channel name>";  
const string SubscriptionName = "<your subscription name>";
//Subscription client
static ISubscriptionClient subscriptionClient;

static async Task Main(string[] args)
{
    // We define our new subscription client
    subscriptionClient = new SubscriptionClient(ServiceBusConnectionString, TopicName,SubscriptionName);

    // Configure the MessageHandler Options in terms of exception handling, number of concurrent messages to deliver etc.
    // By default message will be completed on receive

    var messageHandlerOptions = new MessageHandlerOptions(ExceptionReceivedHandler)
    {
        // Maximum number of Concurrent calls to the callback `ProcessMessagesAsync`, set to 1 for simplicity.
        // Set it according to how many messages the application wants to process in parallel.        
        MaxConcurrentCalls = 1
    };

    // Register the function that will process messages
    subscriptionClient.RegisterMessageHandler(ProcessMessagesAsync, messageHandlerOptions);

    Console.ReadKey();
    // We need to close the consumer to stop receiving and inform Service Bus broker
    await subscriptionClient.CloseAsync();
}
```

Our new handler
```cs
static async Task ProcessMessagesAsync(Message message, CancellationToken token)
{
    await Task.Run(()=>{});
    // We don't want to get an exception if the property is not there. We provide a default alternative
    var user = message.UserProperties["User"] ?? "Unknown";
    // Process the message
    Console.WriteLine($"[{user}]:{Encoding.UTF8.GetString(message.Body)}"); 
}

static Task ExceptionReceivedHandler(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
{
    Console.WriteLine($"Message handler encountered an exception {exceptionReceivedEventArgs.Exception}.");   
    return Task.CompletedTask;
}

```



#### Previous: [WorkQueues &laquo;](./WorkQueues.md)
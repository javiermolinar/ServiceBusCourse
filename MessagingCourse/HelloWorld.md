# Intro to Messaging

#### Prerequisites

Azure Service Bus is a message broker: it accepts and forwards messages. You can think about it as a post office: when you put the mail that you want posting in a post box, you can be sure that Mr. or Ms. Mailperson will eventually deliver the mail to your recipient. In this analogy, Service Bus is a post box, a post office and a postman.

The major difference between ServiceBus and the post office is that it doesn't deal with paper, instead it accepts, stores and forwards binary blobs of data ‒ messages.

#### Jargon

1. **Sender:**  A program that sends messages.
1. **Queue:**  Is the name for a post box which lives inside Azure Service Bus. Although messages flow through ASB and your applications, they can only be stored inside a queue. A queue is only bound by the host's memory & disk limits, it's essentially a large message buffer. Many producers can send messages that go to one queue, and many consumers can try to receive data from one queue. This is how we represent a queue.
1. **Consumer:** A consumer is a program that mostly waits to receive messages.

## Hello World

We'll write two programs in C#; a sender that sends a single message, and a consumer that receives messages and prints them out. It's a "Hello World" of messaging.

<p align ="center">
<img src ="https://docs.microsoft.com/en-us/azure/includes/media/howto-service-bus-queues/sb-queues-08.png" >
</p>

#### Setup

You have two options, either do it by yourself or use this integrated tutorial :)

First lets verify that you have .NET Core toolchain in `PATH`:

``` cs
dotnet --help
```
should produce a help message.

Now let's generate two projects, one for the publisher and one for the consumer:

``` cs
dotnet new console --name Send
mv Send/Program.cs Send/Send.cs
dotnet new console --name Receive
mv Receive/Program.cs Receive/Receive.cs
```

This will create two new directories named Send and Receive.

Then we add the client dependency.

``` cs
cd Send
dotnet add package Microsoft.Azure.ServiceBus 
dotnet restore
cd ../Receive
dotnet add package Microsoft.Azure.ServiceBus
dotnet add package RabbitMQ.Client
dotnet restore
```

Now we have the .NET project set up we can write some code.


#### Sending

We'll call our message publisher (sender) `Send.cs` and our message consumer (receiver) `Receive.cs`. The publisher will connect to ServiceBus, send a single message, then exit.

In `Send.cs`, we need to use some namespaces:

```cs
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
```
and define some properties:

```cs
const string ServiceBusConnectionString = "<your_connection_string>";
const string QueueName = "<your_queue_name>";
static IQueueClient queueClient;
```

then we can create a connection to the server:

```cs
queueClient = new QueueClient(ServiceBusConnectionString, QueueName);
```

The connection abstracts the socket connection, and takes care of protocol version negotiation and authentication and so on for us. Here we connect to a broker. For this we need to specify the connection string and the queue we are going to connect to. The connection string contains all the requiered information about where the broker is placed and the credentials.

To send, we must declare a queue for us to send to; then we can publish a message to the queue:

```cs
try
{
    string messageBody = $"Hello World!";
    var message = new Message(Encoding.UTF8.GetBytes(messageBody));

    // Write the body of the message to the console
    Console.WriteLine($"Sending message: {messageBody}");

    // Send the message to the queue
    await queueClient.SendAsync(message);
}
catch (Exception exception)
{
    Console.WriteLine($"{DateTime.Now} :: Exception: {exception.Message}");
}   
```

In service bus the queue must exists prior to sending messages. On other implementations as RabbitMQ the queue is idempotent and it will be created if doesn't exists.


When the code above finishes running, the channel and the connection will be disposed. That's it for our publisher.

Just click the `run` icon to get started

``` cs  --source-file .\HelloWorld\Sender\Program.cs --project .\HelloWorld\Sender\Sender.csproj 
```
Congratulations! You've sent your very first message!!!


#### Receiving

As for the consumer, it listening for messages from Service Bus. So unlike the publisher which publishes a single message, we'll keep the consumer running continuously to listen for messages and print them out.

The code (in `Receive.cs`) has almost the same using statements as Send:

```cs
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
```

Setting up is the same as the publisher; we open a connection and a channel, and declare the queue from which we're going to consume. Note this matches up with the queue that Send publishes to.

```cs
const string ServiceBusConnectionString = "<your_connection_string>";
const string QueueName = "<your_queue_name>";
static IQueueClient queueClient;
```

then we can create a connection to the server:

```cs
queueClient = new QueueClient(ServiceBusConnectionString, QueueName);
```

We're about to tell the server to deliver us the messages from the queue. Since it will push us messages asynchronously, we provide a callback. 
Besides the callback, the consumer requires some configuration about how is going to consume the messages:

```cs
// Configure the MessageHandler Options in terms of exception handling, number of concurrent messages to deliver etc.
var messageHandlerOptions = new MessageHandlerOptions(ExceptionReceivedHandler)
{
    // Maximum number of Concurrent calls to the callback `ProcessMessagesAsync`, set to 1 for simplicity.
    // Set it according to how many messages the application wants to process in parallel.
    MaxConcurrentCalls = 1
};
```
We need to provide a function that will be executed in case of error consuming the messages:

```cs
static Task ExceptionReceivedHandler(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
{
    Console.WriteLine($"Message handler encountered an exception {exceptionReceivedEventArgs.Exception}.");   
    return Task.CompletedTask;
}
```

And the callback method that will be executed for every message consumed

```cs
static async Task ProcessMessagesAsync(Message message, CancellationToken token)
{
    // Process the message
    Console.WriteLine($"Received message: SequenceNumber:{message.SystemProperties.SequenceNumber} Body:{Encoding.UTF8.GetString(message.Body)}"); 
}
```

Now we are ready to start consuming messages, on the main method:

```cs
// Register the function that will process messages
queueClient.RegisterMessageHandler(ProcessMessagesAsync, messageHandlerOptions);
Console.ReadKey();
// We need to close the consumer to stop receiving and inform Service Bus broker
await queueClient.CloseAsync();
```

**Important**
If you are using the integrated editor you need to do a hack to keep it alive

```cs
//Instead of this
//Console.ReadKey();
var startTime = DateTime.UtcNow;
while(DateTime.UtcNow - startTime < TimeSpan.FromSeconds(10))
{
    // We do nothing, just let the application alive
}   

// We need to close the consumer to stop receiving and inform Service Bus broker
await queueClient.CloseAsync();
```



Just click the `run` icon to get started

``` cs  --source-file .\HelloWorld\\Receiver\Program.cs --project .\HelloWorld\Receiver\Receiver.csproj 
```
Congratulations! You've received your firsts messages!!!

#### Next: [WorkQueues  &raquo;](./WorkQueues.md) Previous: [Home &laquo;](../Readme.md)
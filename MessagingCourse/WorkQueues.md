## Work Queues

In the first tutorial we wrote programs to send and receive messages from a named queue. In this one we'll create a Work Queue that will be used to distribute time-consuming tasks among multiple workers.

![GitHub Logo](./images/workingqueues.png)

The main idea behind Work Queues is to avoid doing a resource-intensive task immediately and having to wait for it to complete. Instead we schedule the task to be done later. We encapsulate a task as a message and send it to a queue. A worker process running in the background will pop the tasks and eventually execute the job. When you run many workers the tasks will be shared between them.

This concept is especially useful in web applications where it's impossible to handle a complex task during a short HTTP request window.

### Preparation

In the previous part of this tutorial we sent a message containing "Hello World!". Now we'll be sending strings that stand for complex tasks. We don't have a real-world task, like images to be resized or pdf files to be rendered, so let's fake it by just pretending we're busy - by using the Thread.Sleep() function (you will need to add using System.Threading; near the top of the file to get access to the threading APIs). We'll take the number of letters in the string as its complexity; every letter will account for one second of "work". For example, a fake task described by Hello will take four seconds.

We will slightly modify the Send program from our previous example, to allow arbitrary messages to be sent from the command line. 

### Sender

As on the first tutorial create a new Sender application and connect it to our queue.
We will create a collection of words with different leght so every one of this will required a different processing time.
The sender will be sent random words from the collection during the time specify on the TimeSpan


```cs

queueClient = new QueueClient(ServiceBusConnectionString, QueueName);        
try
{
    string [] messages = {"a", "test","new","longer"};

    var startTime = DateTime.UtcNow;
    while(DateTime.UtcNow - startTime < TimeSpan.FromSeconds(10))
    {
        Random rand = new Random();                  
        string msg = messages[rand.Next(0,messages.Length)];  
        var message = new Message(Encoding.UTF8.GetBytes(msg));
        Console.WriteLine($"Sending message: {msg}");
        await queueClient.SendAsync(message);
    } 
    Console.WriteLine("Finish sending messages");               
}
catch (Exception exception)
{
    Console.WriteLine($"{DateTime.Now} :: Exception: {exception.Message}");
}   

```

When the code above finishes running, the channel and the connection will be disposed. That's it for our publisher.


### Receiver

#### Message acknowledgment

Doing a task can take a few seconds. What happens if one of the consumers starts a long task and dies with it only partly done? With our previous code, once Azure Service Bus delivers a message to the consumer it immediately marks it for deletion. In this case, if you kill a worker we will lose the message it was just processing. We'll also lose all the messages that were dispatched to this particular worker but were not yet handled.

But we don't want to lose any tasks. If a worker dies, we'd like the task to be delivered to another worker.

In order to make sure a message is never lost, Azure Service Bus supports message acknowledgments. An ack(nowledgement) is sent back by the consumer to tell the broker that a particular message has been received, processed and that Azure Service Bus is free to delete it.

If a consumer dies (its channel is closed, connection is closed, or TCP connection is lost) without sending an ack, Service Bus will understand that a message wasn't processed fully and will re-queue it. If there are other consumers online at the same time, it will then quickly redeliver it to another consumer. That way you can be sure that no message is lost, even if the workers occasionally die.

As on the first tutorial create a new Sender application and connect it to our queue.
We will create a collection of words with different leght so every one of this will required a different processing time.
The sender will be sent random words from the collection during the time specify on the TimeSpan

#### Implementation

As on the first tutorial create a new Receiver application and connect it to our queue.
This time we will introduce a delay using Thread.Sleep to replicate program that takes some time to process a message. We will take the lengh of the message body for that

```cs

       
queueClient = new QueueClient(ServiceBusConnectionString, QueueName);  
var messageHandlerOptions = new MessageHandlerOptions(ExceptionReceivedHandler)
{
    // Maximum number of Concurrent calls to the callback `ProcessMessagesAsync`, set to 1 for simplicity.
    // Set it according to how many messages the application wants to process in parallel.
    MaxConcurrentCalls = 1,

        // Indicates whether ServiceBus Sdk should complete  or not the message when message is received.  
    AutoComplete = false
};

// Register the function that will process messages
queueClient.RegisterMessageHandler(ProcessMessagesAsync, messageHandlerOptions);


Console.ReadKey();
// We need to close the consumer to stop receiving and inform Service Bus broker
await queueClient.CloseAsync();




static async Task ProcessMessagesAsync(Message message, CancellationToken token)
{
string msg = Encoding.UTF8.GetString(message.Body);
// Process the message
Console.WriteLine($"Received message: SequenceNumber:{message.SystemProperties.SequenceNumber} Body:{msg}");

Thread.Sleep(msg.Length * 1000);

Console.WriteLine("Finish processing");

// Complete the message so that it is not received again.
// This can be done only if the queueClient is created in ReceiveMode.PeekLock mode (which is default).
await queueClient.CompleteAsync(message.SystemProperties.LockToken);       
}

static Task ExceptionReceivedHandler(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
{
Console.WriteLine($"Message handler encountered an exception {exceptionReceivedEventArgs.Exception}.");            
return Task.CompletedTask;
}
```

Questions:

- How could we improve the performace for a single receiver?
- What happen if we run several receivers at the same time? Can the receivers get duplicated messages?
- What happen now if we get and exception processing the message?

#### Next: [Publish-Subscribe  &raquo;](./PublishSubscribe.md) Previous: [Home &laquo;](./HelloWorld.md)
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


### Let's play a game



#### Previous: [WorkQueues &laquo;](./WorkQueues.md)
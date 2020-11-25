using Microsoft.Azure.ServiceBus;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


const string ServiceBusConnectionString = "Endpoint=sb://messagingcourse.servicebus.windows.net/;SharedAccessKeyName=course;SharedAccessKey=ramt/bWgACAYJq/S7zjMcjCTNGp6zxjWe3Q6hBot7CI=";
const string TopicName = "chat";
const string SubscriptionName = "yoursubscription";
//Subscription client
ISubscriptionClient subscriptionClient;

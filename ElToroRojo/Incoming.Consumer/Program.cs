using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

using var cancellationTokenSource = new CancellationTokenSource();

using var timer = new Timer(_ =>
{
    cancellationTokenSource.Cancel();
}, null, TimeSpan.FromMinutes(1), Timeout.InfiniteTimeSpan);

var connectionFactory = new ConnectionFactory
{
    UserName = "admin",
    Password = "admin",
    VirtualHost = "/",
    HostName = "incoming.queue",
    Port = 5672,
    ClientProvidedName = "Consumer",
    DispatchConsumersAsync = true
};

using var connection = connectionFactory.CreateConnection();
using var channel = connection.CreateModel();
Console.WriteLine("Connected to route: test-key.");

channel.BasicQos(0, 1, false);
var eventsConsumer = new AsyncEventingBasicConsumer(channel);
channel.BasicConsume("test-queue", false, eventsConsumer);

eventsConsumer.Received += async (_, payload) =>
{
    try
    {
        await Task.Run(() =>
        {
            var payloadAsString = Encoding.UTF8.GetString(payload.Body.ToArray());
            Console.WriteLine(payloadAsString);
            channel.BasicAck(payload.DeliveryTag, false);
        });
    }
    catch (Exception e)
    {
        Console.WriteLine(e);
        channel.BasicReject(payload.DeliveryTag, false);
    }

    timer.Change(TimeSpan.FromMinutes(1), Timeout.InfiniteTimeSpan);
};

Console.WriteLine("Subscribed to queue: test-queue.");

channel.Close();
connection.Close();
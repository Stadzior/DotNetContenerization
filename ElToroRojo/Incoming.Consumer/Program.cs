using System;
using System.Data.SqlClient;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

var fileName = args[0];
var apiUri = $"http://incoming.api-{fileName}:80";
const string connectionString = "data source=errordb,1433;initial catalog=ErrorDb;user id=sa;password=zaq1@WSX;integrated security=false;";
using var cancellationTokenSource = new CancellationTokenSource();

using var timer = new Timer(_ =>
{
    cancellationTokenSource.Cancel();
}, null, TimeSpan.FromSeconds(10), Timeout.InfiniteTimeSpan);

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

using var queueConnection = connectionFactory.CreateConnection();
using var channel = queueConnection.CreateModel();
Console.WriteLine($"Connected to route: test-key. {DateTime.Now.ToLongTimeString()}");

channel.BasicQos(0, 1, false);
var eventsConsumer = new AsyncEventingBasicConsumer(channel);
channel.BasicConsume("test-queue", false, eventsConsumer);
Console.WriteLine($"Subscribed to queue: test-queue. {DateTime.Now.ToLongTimeString()}");

var currentlyProcessing = false;

eventsConsumer.Received += async (_, payload) =>
{
    currentlyProcessing = true;
    try
    {
        await ProcessMessage(payload);
        channel.BasicAck(payload.DeliveryTag, false);
    }
    catch (Exception e)
    {
        Console.WriteLine(e);
        channel.BasicReject(payload.DeliveryTag, false);
    }

    timer.Change(TimeSpan.FromSeconds(10), Timeout.InfiniteTimeSpan);

    currentlyProcessing = false;
};

while (!cancellationTokenSource.IsCancellationRequested || currentlyProcessing) 
    Thread.Sleep(TimeSpan.FromSeconds(1));

Console.WriteLine($"Closing connection to route: test-key. {DateTime.Now.ToLongTimeString()}");

channel.Close();
queueConnection.Close();

async Task ProcessMessage(BasicDeliverEventArgs payload)
{
    var payloadAsString = Encoding.UTF8.GetString(payload.Body.ToArray());
    Console.WriteLine($"Processing message: {payloadAsString}. {DateTime.Now.ToLongTimeString()}");

    var request = new HttpRequestMessage(HttpMethod.Post, $@"{apiUri}/items/add")
    {
        Content = JsonContent.Create(payloadAsString)
    };

    var httpClient = new HttpClient();
    var response = await httpClient.SendAsync(request);
    
    if (response.StatusCode == HttpStatusCode.Created)
        Console.WriteLine($"Message successfully processed: {payloadAsString}. {DateTime.Now.ToLongTimeString()}");
    else
    {
        var error = await response.Content.ReadAsStringAsync();
        AddErrorToDatabase(payloadAsString, error);
    }
}

void AddErrorToDatabase(string payload, string error)
{
    using var dbConnection = new SqlConnection(connectionString);
    var command = new SqlCommand($"INSERT INTO dbo.Errors (Payload, Description) Values ('{payload}','{error}');", dbConnection);
    try
    {
        dbConnection.Open();
        command.ExecuteNonQuery();
    }
    catch (Exception e)
    {
        Console.WriteLine(e.Message);
    }
}
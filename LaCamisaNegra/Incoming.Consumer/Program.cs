using System;
using System.Data.SqlClient;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Threading;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

const string apiUri = $"http://host.docker.internal:5001";
const string connectionString = @"data source=host.docker.internal,1434;initial catalog=ErrorDb;user id=sa;password=zaq1@WSX;integrated security=false;";
using var cancellationTokenSource = new CancellationTokenSource();

var connectionFactory = new ConnectionFactory
{
    UserName = "admin",
    Password = "admin",
    VirtualHost = "/",
    HostName = "host.docker.internal",
    Port = 5673,
    ClientProvidedName = "Consumer",
    DispatchConsumersAsync = true
};

using var queueConnection = connectionFactory.CreateConnection();
using var channel = queueConnection.CreateModel();
Console.WriteLine($"Connected to incoming.queue. {DateTime.Now.ToLongTimeString()}");

channel.BasicQos(0, 1, false);
var eventsConsumer = new AsyncEventingBasicConsumer(channel);
channel.BasicConsume($"incoming-queue", false, eventsConsumer);
Console.WriteLine($"Subscribed to queue: incoming-queue. {DateTime.Now.ToLongTimeString()}");

eventsConsumer.Received += async (_, payload) =>
{
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
};

while (true) {  }

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
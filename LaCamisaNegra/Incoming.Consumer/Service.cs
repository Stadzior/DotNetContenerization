using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client.Events;
using System.Data.SqlClient;
using System.Net.Http.Json;
using System.Net;

namespace Incoming.Consumer;
public class Service : BackgroundService
{
    private readonly string apiUri;
    private const string connectionString = "data source=errordb,1433;initial catalog=ErrorDb;user id=sa;password=zaq1@WSX;integrated security=false;";

    protected virtual Timer? Timer { get; set; }

    public Service(string fileName)
    {
        apiUri = $"http://incoming.api-{fileName}:80";
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Console.WriteLine($"Service has started to work at: {DateTime.Now.ToLongTimeString()}");
        await Task.Factory.StartNew(() => RunProcess(), stoppingToken);
    }

    public void RunProcess()
    {
        using var cancellationTokenSource = new CancellationTokenSource();
        
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
    }

    public async Task ProcessMessage(BasicDeliverEventArgs payload)
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

    public void AddErrorToDatabase(string payload, string error)
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
}
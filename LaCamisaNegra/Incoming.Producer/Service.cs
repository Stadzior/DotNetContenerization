using RabbitMQ.Client;
using System.Text;
using Microsoft.Extensions.Hosting;

namespace Incoming.Producer;

public class Service : BackgroundService
{
    private readonly string filePath;
    protected virtual Timer? Timer { get; set; }

    public Service(string filePath)
    {
        this.filePath = filePath;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Console.WriteLine($"Service has started to work at: {DateTime.Now.ToLongTimeString()}");
        await Task.Factory.StartNew(() => RunEvery(), stoppingToken);
    }

    public virtual void RunEvery(int minutes = 1, int seconds = 0, int milliseconds = 0)
    {
        try
        {
            var period = TimeSpan
            .FromMinutes(minutes)
            .Add(TimeSpan.FromSeconds(seconds))
            .Add(TimeSpan.FromMilliseconds(milliseconds));

            Timer = new Timer(_ => RunProcess(), null, TimeSpan.Zero, period);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }

    public void RunProcess()
    {
        Console.WriteLine($"Processing file: {filePath}. {DateTime.Now.ToLongTimeString()}");

        if (File.Exists(filePath))
        {
            var rows = File.ReadAllLines(filePath);

            var connectionFactory = new ConnectionFactory
            {
                UserName = "admin",
                Password = "admin",
                VirtualHost = "/",
                HostName = "incoming.queue",
                Port = 5672,
                ClientProvidedName = "Producer"
            };

            using var connection = connectionFactory.CreateConnection();
            using var channel = connection.CreateModel();
            Console.WriteLine($"Connected to route: test-key. {DateTime.Now.ToLongTimeString()}");

            var properties = channel.CreateBasicProperties();
            properties.Persistent = true;

            foreach (var row in rows)
            {
                channel.BasicPublish("test-exchange", "test-key", true, properties, Encoding.UTF8.GetBytes(row));
                Console.WriteLine($"Sending message: {row}, from file: {filePath}, to route: test-key. {DateTime.Now.ToLongTimeString()}");
            }

            File.Delete(filePath);
            Console.WriteLine($"File: {filePath} deleted. {DateTime.Now.ToLongTimeString()}");

            channel.Close();
            connection.Close();
        }
        else
            Console.WriteLine($"File: {filePath} does not exist. {DateTime.Now.ToLongTimeString()}");
    }
}
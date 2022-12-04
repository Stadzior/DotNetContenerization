using System.Text;
using RabbitMQ.Client;

var filePath = args[0];
Console.WriteLine($"Processing file: {filePath}. {DateTime.Now.ToLongTimeString()}");

if (File.Exists(filePath))
{
    var rows = File.ReadAllLines(filePath);

    var connectionFactory = new ConnectionFactory
    {
        UserName = "guest",
        Password = "guest",
        VirtualHost = "/",
        HostName = "localhost",
        Port = 1434,
        ClientProvidedName = "Producer",
        DispatchConsumersAsync = true
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

using System;
using System.IO;
using System.Text;
using RabbitMQ.Client;

var connectionFactory = new ConnectionFactory
{
    UserName = "admin",
    Password = "admin",
    VirtualHost = "/",
    HostName = "host.docker.internal",
    Port = 5673,
    ClientProvidedName = "Producer"
};

using var connection = connectionFactory.CreateConnection();
using var channel = connection.CreateModel();
Console.WriteLine($"Connected to route: incoming-key. {DateTime.Now.ToLongTimeString()}");

var properties = channel.CreateBasicProperties();
properties.Persistent = true;

const string workspaceDir = @"C:\workspace";

while (true)
{
    var files = Directory.GetFiles(workspaceDir);
    
    foreach (var filePath in files)
    {
        Console.WriteLine($"Processing file: {filePath}. {DateTime.Now.ToLongTimeString()}");

        var rows = File.ReadAllLines(filePath);
        var fileName = Path.GetFileName(filePath);

        foreach (var row in rows)
        {
            channel.BasicPublish("incoming-exchange", "incoming-key", true, properties, Encoding.UTF8.GetBytes(row));
            Console.WriteLine($"Sending message: {row}, from file: {filePath}, to route: incoming-key. {DateTime.Now.ToLongTimeString()}");
        }

        File.Delete(filePath);
        Console.WriteLine($"File: {filePath} deleted. {DateTime.Now.ToLongTimeString()}");
    }

    Thread.Sleep(TimeSpan.FromSeconds(5));
}
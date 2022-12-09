using System;
using System.Data.SqlClient;
using System.Threading;

var connectionString = @"data source=host.docker.internal,1434;initial catalog=ItemDb;user id=sa;password=zaq1@WSX;integrated security=false;";
Console.WriteLine(connectionString);
while (true)
{
    var message = $"Item added at: {DateTime.Now.ToLongTimeString()}";
    AddMessageToDatabase(message);
    Console.WriteLine(message);
    Thread.Sleep(TimeSpan.FromSeconds(5));
}

void AddMessageToDatabase(string message)
{
    using var connection = new SqlConnection(connectionString);
    var command = new SqlCommand($"INSERT INTO dbo.Items (Content) Values ('{message}');", connection);
    try
    {
        connection.Open();
        command.ExecuteNonQuery();
    }
    catch (Exception e)
    {
        Console.WriteLine(e.Message);
    }
}
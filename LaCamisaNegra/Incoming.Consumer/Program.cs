using System;
using System.Data.SqlClient;
using System.Threading;

const string connectionString = "data source=itemdb,1434;initial catalog=ItemDb;user id=sa;password=zaq1@WSX;integrated security=false;";

while (true)
{
    AddMessageToDatabase($"Heheszki {DateTime.Now.ToLongTimeString()}");
    Console.WriteLine("LOL");
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
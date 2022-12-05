using System.Data.SqlClient;
using Microsoft.AspNetCore.Mvc;

const string connectionString = "data source=itemdb,1433;initial catalog=ItemDb;user id=sa;password=zaq1@WSX;integrated security=false;";

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapPost("/items/add", ([FromBody] string request, HttpContext httpContext) =>
{
    if (request.Contains("error", StringComparison.InvariantCultureIgnoreCase))
    {
        httpContext.Response.StatusCode = 500;
        return $"Error messages are not tolerated by this api! Message content: {request}. {DateTime.Now.ToLongTimeString()}";
    }

    AddMessageToDatabase(request);
    httpContext.Response.StatusCode = 201;
    return $"Message added to the target database: {request}. {DateTime.Now.ToLongTimeString()}";
});

app.Run();

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
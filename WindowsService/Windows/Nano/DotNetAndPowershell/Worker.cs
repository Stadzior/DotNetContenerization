namespace WindowsService;

public class Worker : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            Console.WriteLine("Running!");
            await Task.Delay(100, stoppingToken);
        }
    }
}

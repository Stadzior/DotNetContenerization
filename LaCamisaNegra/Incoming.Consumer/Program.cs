using Incoming.Consumer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using var host = Host.CreateDefaultBuilder()
    .ConfigureServices(services =>
    {
        services.AddHostedService(_ => new Service(args[0]));
    })
    .Build();

host.Run();
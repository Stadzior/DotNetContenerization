using System.Data;
using Docker.DotNet;
using Docker.DotNet.Models;

const string workspaceDir = "/workspace";

Console.WriteLine($"Watchdog started. {DateTime.Now.ToShortTimeString()}");

while (true)
{
    var files = Directory.GetFiles(workspaceDir);

    if (files.Any())
    {
        Console.WriteLine($"Beginning of processing {files.Length} file(s). {DateTime.Now.ToShortTimeString()}");

        var dockerEngineUrl = new Uri("unix://var/run/docker.sock");
        using var client = new DockerClientConfiguration(dockerEngineUrl).CreateClient();

        var queueContainerId = await CreateQueue(client);

        foreach (var filePath in files)
        {
            var fileName = Path.GetFileName(filePath);

            await CreateApi(client, fileName);
            await CreateConsumer(client, fileName);
            await CreateProducer(client, fileName, filePath);
        }

        var listParameters = new ContainersListParameters { All = true };

        Console.WriteLine($"Waiting for producer/consumer containers to cleanup. {DateTime.Now.ToLongTimeString()}");

        var dynamicContainersExists = true;
        var dynamicContainersPurposes = new[] { "producer", "consumer", "api" };

        while(dynamicContainersExists)
        {
            var containers = await client.Containers.ListContainersAsync(listParameters);
            dynamicContainersExists = containers
                .Any(container => dynamicContainersPurposes.Contains(container.Labels["purpose"]));

            if (dynamicContainersExists)
                Thread.Sleep(TimeSpan.FromSeconds(1));
        }

        Console.WriteLine($"Removing queue container. {DateTime.Now.ToLongTimeString()}");

        var stopParameters = new ContainerStopParameters { WaitBeforeKillSeconds = 10 };

        await client.Containers.StopContainerAsync(queueContainerId, stopParameters);

        Console.WriteLine($"Queue container stopped (and automatically removed). {DateTime.Now.ToLongTimeString()}");
    }
}

async Task<string> CreateQueue(IDockerClient client)
{
    var parameters = new CreateContainerParameters
    {
        Name = "incoming.queue",
        Image = "rabbitmq:3.11-management",
        Hostname = "incoming.queue",
        ExposedPorts = new Dictionary<string, EmptyStruct>
        {
            {"15672", default}
        },
        HostConfig = new HostConfig
        {
            AutoRemove = true,
            PortBindings = new Dictionary<string, IList<PortBinding>>
            {
                {"15672", new List<PortBinding> { new() { HostPort = "15673" } }}
            },
            NetworkMode = "eltororojo_incoming-queue-network"
        },
        Labels = new Dictionary<string, string>
        {
            {"purpose", "queue"},
            {"case", "queue"},
            {"apptype", "consoleapp"}
        }
    };

    var containerId = await CreateContainer(client, parameters, "Queue");

    await SetupRabbitMq(client, containerId);

    return containerId;
}

async Task SetupRabbitMq(IDockerClient client, string containerId)
{
    await ExecCommandInsideContainer(client, containerId, "rabbitmqadmin list exchanges", 10);
    Console.WriteLine($"RabbitMq is ready to use. {DateTime.Now.ToLongTimeString()}");

    await ExecCommandInsideContainer(client, containerId, "rabbitmqadmin declare exchange name=test-exchange type=direct");
    Console.WriteLine($"Declared exchange inside queue container with id: {containerId} {DateTime.Now.ToLongTimeString()}");

    await ExecCommandInsideContainer(client, containerId, "rabbitmqadmin declare queue name=test-queue durable=false");
    Console.WriteLine($"Declared queue inside queue container with id: {containerId} {DateTime.Now.ToLongTimeString()}");

    await ExecCommandInsideContainer(client, containerId, "rabbitmqadmin declare binding source=test-exchange destination_type=queue destination=test-queue routing_key=test-key");
    Console.WriteLine($"Declared binding inside queue container with id: {containerId} {DateTime.Now.ToLongTimeString()}");

    await ExecCommandInsideContainer(client, containerId, "rabbitmqctl add_user admin admin");
    Console.WriteLine($"Created user inside queue container with id: {containerId} {DateTime.Now.ToLongTimeString()}");

    await ExecCommandInsideContainer(client, containerId, "rabbitmqctl set_user_tags admin administrator");
    Console.WriteLine($"Made that new user an admin inside queue container with id: {containerId} {DateTime.Now.ToLongTimeString()}");
    
    await ExecCommandInsideContainer(client, containerId, "rabbitmqctl set_permissions -p / admin .* .* .*");
    Console.WriteLine($"Set permissions for an admin inside queue container with id: {containerId} {DateTime.Now.ToLongTimeString()}");
}

async Task ExecCommandInsideContainer(IDockerClient client, string containerId, string command, int retryCount = 0)
{
    for (var i = 0; i <= retryCount; i++)
    {
        var execParameters = new ContainerExecCreateParameters
        {
            Cmd = command.Split(' ').ToList(),
            AttachStdin = true,
            AttachStderr = true
        };

        var exec = await client.Exec.ExecCreateContainerAsync(containerId, execParameters);

        using var execStream = await client.Exec.StartAndAttachContainerExecAsync(exec.ID, false);
        await execStream
            .ReadOutputToEndAsync(new CancellationToken())
            .ConfigureAwait(false);
        var execInspectResponse = await client.Exec
            .InspectContainerExecAsync(exec.ID)
            .ConfigureAwait(false);

        if (execInspectResponse.ExitCode == 0)
            break;

        Console.WriteLine($"Failed execution of '{command}' ({i+1}/{retryCount}). {DateTime.Now.ToLongTimeString()}");
        Thread.Sleep(TimeSpan.FromSeconds(1));
    }
}

async Task CreateApi(IDockerClient client, string fileName)
{
    var parameters = new CreateContainerParameters
    {
        Name = $"incoming.api-{fileName}",
        Image = "incoming.api:latest",
        HostConfig = new HostConfig
        {
            AutoRemove = true
        },
        NetworkingConfig = new NetworkingConfig
        {
            EndpointsConfig = new Dictionary<string, EndpointSettings>
            {
                {"eltororojo_incoming-api-network", new EndpointSettings()},
                {"eltororojo_itemdb-network", new EndpointSettings()}
            }
        }
    };

    await CreateContainer(client, parameters, "Api", fileName);
}

async Task CreateConsumer(IDockerClient client, string fileName)
{
    var parameters = new CreateContainerParameters
    {
        Name = $"incoming.consumer-{fileName}",
        Image = "incoming.consumer:latest",
        HostConfig = new HostConfig
        {
            AutoRemove = true
        },
        NetworkingConfig = new NetworkingConfig
        {
            EndpointsConfig = new Dictionary<string, EndpointSettings>
            {
                {"eltororojo_incoming-queue-network", new EndpointSettings()},
                {"eltororojo_incoming-api-network", new EndpointSettings()},
                {"eltororojo_errordb-network", new EndpointSettings()},
            }
        }
    };

    await CreateContainer(client, parameters, "Consumer", fileName);
}

async Task CreateProducer(IDockerClient client, string fileName, string filePath)
{
    var parameters = new CreateContainerParameters
    {
        Name = $"incoming.producer-{fileName}",
        Image = "incoming.producer:latest",
        HostConfig = new HostConfig
        {
            AutoRemove = true,
            Binds = new List<string>
            {
                @"C:\workspace:/workspace:rw"
            },
            NetworkMode = "eltororojo_incoming-queue-network"
        },
        Env = new List<string>
        {
            $"FILE_PATH={filePath}"
        }
    };

    await CreateContainer(client, parameters, "Producer", fileName);
}

async Task<string> CreateContainer(IDockerClient client, CreateContainerParameters parameters, string containerType, string? fileName = null)
{
    var container = await client.Containers.CreateContainerAsync(parameters);
    
    Console.WriteLine($"{containerType}({fileName}) container created with id: {container.ID}. {DateTime.Now.ToLongTimeString()}");

    await client.Containers.StartContainerAsync(container.ID, new ContainerStartParameters());
    Console.WriteLine($"{containerType}({fileName}) container started with id: {container.ID} {DateTime.Now.ToLongTimeString()}");

    return container.ID;
}
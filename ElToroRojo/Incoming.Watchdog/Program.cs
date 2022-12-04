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

        var queueParameters = new CreateContainerParameters
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
                //AutoRemove = true,
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

        var queueContainer = await client.Containers.CreateContainerAsync(queueParameters);

        await client.Containers.StartContainerAsync(queueContainer.ID, new ContainerStartParameters());
        Console.WriteLine($"Started queue container with id: {queueContainer.ID}. {DateTime.Now.ToLongTimeString()}");

        await SetupRabbitMq(client, queueContainer.ID);

        foreach (var filePath in files)
        {
            var fileName = Path.GetFileName(filePath);

            var consumerParameters = new CreateContainerParameters
            {
                Name = $"incoming.consumer-{fileName}",
                Image = "incoming.consumer:latest",
                HostConfig = new HostConfig
                {
                    //AutoRemove = true,
                    NetworkMode = "eltororojo_incoming-queue-network"
                }
            };
            
            var consumerContainer = await client.Containers.CreateContainerAsync(consumerParameters);
            Console.WriteLine($"Consumer container with id: {consumerContainer.ID}, created for file: {fileName} {DateTime.Now.ToLongTimeString()}");

            await client.Containers.StartContainerAsync(consumerContainer.ID, new ContainerStartParameters());
            Console.WriteLine($"Started consumer container with id: {consumerContainer.ID} {DateTime.Now.ToLongTimeString()}");

            var producerParameters = new CreateContainerParameters
            {
                Name = $"incoming.producer-{fileName}",
                Image = "incoming.producer:latest",
                HostConfig = new HostConfig
                {
                    //AutoRemove = true,
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

            var producerContainer = await client.Containers.CreateContainerAsync(producerParameters);
            Console.WriteLine($"Producer container with id: {producerContainer.ID}, created for file: {fileName} {DateTime.Now.ToLongTimeString()}");
            
            await client.Containers.StartContainerAsync(producerContainer.ID, new ContainerStartParameters());
            Console.WriteLine($"Started producer container with id: {producerContainer.ID} {DateTime.Now.ToLongTimeString()}");
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

        await client.Containers.StopContainerAsync(queueContainer.ID, stopParameters);

        Console.WriteLine($"Queue container stopped (and automatically removed). {DateTime.Now.ToLongTimeString()}");
    }
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
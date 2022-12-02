using Docker.DotNet;
using Docker.DotNet.Models;
using System.Security.Cryptography;

const string workspaceDir = "/workspace";

while(true)
{
    Console.WriteLine($"Beginning of processing at {DateTime.Now.ToShortTimeString()}.");

    var files = Directory.GetFiles(workspaceDir);

    if (files.Any())
    {
        var dockerEngineUrl = new Uri("unix://var/run/docker.sock");
        using var client = new DockerClientConfiguration(dockerEngineUrl).CreateClient();

        var queueParameters = new CreateContainerParameters
        {
            Name = "incoming.queue",
            Image = "rabbitmq:3.11-management",
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
                }
            },
            Labels = new Dictionary<string, string>
            {
                {"purpose", "queue"},
                {"case", "queue"},
                {"apptype", "consoleapp"}
            }
        };

        var queueContainer = await client.Containers.CreateContainerAsync(queueParameters);
        Console.WriteLine($"Queue container with id: {queueContainer.ID}");

        await client.Containers.StartContainerAsync(queueContainer.ID, new ContainerStartParameters());
        Console.WriteLine($"Started queue container with id: {queueContainer.ID}");

        var execDeclareExchangeParameters = new ContainerExecCreateParameters
        {
            Cmd = new List<string> { "rabbitmqadmin", "declare", "exchange", "name=test-exchange","type=direct" }
        };
        var execDeclareExchange = await client.Exec.ExecCreateContainerAsync(queueContainer.ID, execDeclareExchangeParameters);
        await client.Exec.StartAndAttachContainerExecAsync(execDeclareExchange.ID, false);
        Console.WriteLine($"Declared exchange inside queue container with id: {queueContainer.ID}");

        var execDeclareQueueParameters = new ContainerExecCreateParameters
        {
            Cmd = new List<string> { "rabbitmqadmin" ,"declare" ,"queue" ,"name=test-queue" ,"durable=false" }
        };
        var execDeclareQueue = await client.Exec.ExecCreateContainerAsync(queueContainer.ID, execDeclareQueueParameters);
        await client.Exec.StartAndAttachContainerExecAsync(execDeclareQueue.ID, false);
        Console.WriteLine($"Declared queue inside queue container with id: {queueContainer.ID}");

        var execDeclareBindingParameters = new ContainerExecCreateParameters
        {
            Cmd = new List<string> { "rabbitmqadmin" ,"declare" ,"binding" ,"source=\"test-exchange\"" ,"destination_type=\"queue\"" ,"destination=\"test-queue\"" ,"routing_key=\"test-key\"" }
        };
        var execDeclareBinding = await client.Exec.ExecCreateContainerAsync(queueContainer.ID, execDeclareBindingParameters);
        await client.Exec.StartAndAttachContainerExecAsync(execDeclareBinding.ID, false);
        Console.WriteLine($"Declared binding inside queue container with id: {queueContainer.ID}");

        foreach (var filePath in files)
        {
            var fileName = Path.GetFileName(filePath);

            var consumerParameters = new CreateContainerParameters
            {
                Name = $"incoming.consumer-{fileName}",
                Image = "incoming.consumer:latest",
                HostConfig = new HostConfig
                {
                    //AutoRemove = true
                }
            };
            
            var consumerContainer = await client.Containers.CreateContainerAsync(consumerParameters);
            Console.WriteLine($"Consumer container with id: {consumerContainer.ID}, created for file: {fileName}");

            await client.Containers.StartContainerAsync(consumerContainer.ID, new ContainerStartParameters());
            Console.WriteLine($"Started consumer container with id: {consumerContainer.ID}");

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
                    }
                },
                Env = new List<string>
                {
                    $"FILE_PATH={filePath}"
                }
            };

            var producerContainer = await client.Containers.CreateContainerAsync(producerParameters);
            Console.WriteLine($"Producer container with id: {producerContainer.ID}, created for file: {fileName}");
            
            await client.Containers.StartContainerAsync(producerContainer.ID, new ContainerStartParameters());
            Console.WriteLine($"Started producer container with id: {producerContainer.ID}");
        }

        var listParameters = new ContainersListParameters { All = true };

        Console.WriteLine("Waiting for producer/consumer containers to remove themselves.");

        var dynamicContainersExists = true;
        var dynamicContainersPurposes = new[] { "producer", "consumer" };

        while(dynamicContainersExists)
        {
            var containers = await client.Containers.ListContainersAsync(listParameters);
            dynamicContainersExists = containers
                .Any(container => dynamicContainersPurposes.Contains(container.Labels["purpose"]));

            if (dynamicContainersExists)
                Thread.Sleep(TimeSpan.FromSeconds(1));
        }

        Console.WriteLine("Removing queue container.");

        var stopParameters = new ContainerStopParameters { WaitBeforeKillSeconds = 10 };

        await client.Containers.StopContainerAsync(queueContainer.ID, stopParameters);

        Console.WriteLine("Queue container stopped (and automatically removed).");
    }
}
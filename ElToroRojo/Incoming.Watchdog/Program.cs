using Docker.DotNet;
using Docker.DotNet.Models;

const string workspaceDir = "/workspace";

while(true)
{
    var files = Directory.GetFiles(workspaceDir);

    if (files.Any())
    {
        var dockerEngineUrl = new Uri("unix://var/run/docker.sock");
        using var client = new DockerClientConfiguration(dockerEngineUrl).CreateClient();

        RunQueueContainer(client);

        foreach (var filePath in files)
        {
            var fileName = Path.GetFileName(filePath);

            var consumerParameters = new CreateContainerParameters
            {
                Name = $"incoming.consumer-{fileName}",
                Image = "incoming.consumer:latest",
                HostConfig = new HostConfig
                {
                    AutoRemove = true
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
                    AutoRemove = true,
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
    }
}

void RunQueueContainer(IDockerClient client)
{

    var queueParameters = new CreateContainerParameters
    {
        Name = "incoming.queue",
        Image = "rabbitmq:3.11-management",
        ExposedPorts = new Dictionary<string, EmptyStruct>
        {
            {"1434:1433", new EmptyStruct()}
        },
        HostConfig = new HostConfig
        {
            AutoRemove = true
        }
    };

    var queueContainer = await client.Containers.CreateContainerAsync(queueParameters);
    Console.WriteLine($"Queue container with id: {queueContainer.ID}");

    await client.Containers.StartContainerAsync(queueContainer.ID, new ContainerStartParameters());
    Console.WriteLine($"Started queue container with id: {queueContainer.ID}");

    var execParameters = new ContainerExecCreateParameters
    {
        Cmd = new List<string>
        {
            "rabbitmqadmin declare exchange name=test-exchange type=direct",
            "rabbitmqadmin declare queue name=test-queue durable=false",
            "rabbitmqadmin declare binding source=\"test-exchange\" destination_type=\"queue\" destination=\"test-queue\" routing_key=\"test-key\""
        }
    };

    await client.Exec.ExecCreateContainerAsync(queueContainer.ID, execParameters);
    Console.WriteLine($"Initialized rabbitmq inside queue container with id: {queueContainer.ID}");
}
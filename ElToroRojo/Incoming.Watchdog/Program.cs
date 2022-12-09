using System.ComponentModel;
using System.Data;
using System.Text;
using Docker.DotNet;
using Docker.DotNet.Models;
using ICSharpCode.SharpZipLib.Tar;

const string workspaceDir = "/workspace";

Console.WriteLine($"Watchdog started. {DateTime.Now.ToLongTimeString()}");
while (true)
{
    var fileNames = Directory
        .GetFiles(workspaceDir)
        .Select(Path.GetFileName)
        .Cast<string>()
        .ToList();

    if (fileNames.Any())
    {
        Console.WriteLine($"Beginning of processing {fileNames.Count} file(s). {DateTime.Now.ToLongTimeString()}");

        var dockerEngineUrl = new Uri("unix://var/run/docker.sock");
        using var client = new DockerClientConfiguration(dockerEngineUrl).CreateClient();
        
        await CreateQueue(client, fileNames);

        foreach (var fileName in fileNames)
        {
            await CreateApi(client, fileName);
            await CreateConsumer(client, fileName);
            await CreateProducer(client, fileName);
        }

        await Cleanup(client, "api", new[] { "consumer" });
        await Cleanup(client, "queue", new[] { "producer", "consumer" });
    }
    else
        Thread.Sleep(TimeSpan.FromSeconds(1));
}

async Task CreateQueue(IDockerClient client, ICollection<string> fileNames)
{
    const string imageName = "rabbitmq";
    const string imageTag = "3.11-management";

    await PullImage(client, imageName, imageTag);

    var parameters = new CreateContainerParameters
    {
        Name = "incoming.queue",
        Image = $"{imageName}:{imageTag}",
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

    await SetupRabbitMq(client, containerId, fileNames);
}

async Task SetupRabbitMq(IDockerClient client, string containerId, ICollection<string> fileNames)
{
    await ExecCommandInsideContainer(client, containerId, "rabbitmqadmin list exchanges", 10);
    Console.WriteLine($"RabbitMq is ready to use. {DateTime.Now.ToLongTimeString()}");

    foreach (var fileName in fileNames)
    {
        await ExecCommandInsideContainer(client, containerId, $"rabbitmqadmin declare exchange name={fileName}-exchange type=direct");
        Console.WriteLine($"Declared exchange '{fileName}-exchange' inside queue container with id: {containerId} {DateTime.Now.ToLongTimeString()}");

        await ExecCommandInsideContainer(client, containerId, $"rabbitmqadmin declare queue name={fileName}-queue durable=false");
        Console.WriteLine($"Declared queue '{fileName}-queue' inside queue container with id: {containerId} {DateTime.Now.ToLongTimeString()}");

        await ExecCommandInsideContainer(client, containerId, $"rabbitmqadmin declare binding source={fileName}-exchange destination_type=queue destination={fileName}-queue routing_key={fileName}-key");
        Console.WriteLine($"Declared binding from: '{fileName}-exchange' to: '{fileName}-queue' with routing key:'{fileName}-key' inside queue container with id: {containerId} {DateTime.Now.ToLongTimeString()}");
    }

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
    const string imageName = "incoming.api";
    const string dockerFilePath = "/builds/Incoming.Api";
    await BuildImage(client, imageName, dockerFilePath);

    var parameters = new CreateContainerParameters
    {
        Name = $"incoming.api-{fileName}",
        Image = imageName,
        Hostname = $"incoming.api-{fileName}",
        ExposedPorts = new Dictionary<string, EmptyStruct>
        {
            {"5001", default}
        },
        HostConfig = new HostConfig
        {
            AutoRemove = true,
            PortBindings = new Dictionary<string, IList<PortBinding>>
            {
                {"80", new List<PortBinding> { new() { HostPort = "5001" } }}
            },
            NetworkMode = "eltororojo_incoming-api-network"
        }
    };

    var containerId = await CreateContainer(client, parameters, "Api", fileName);

    await ConnectToNetwork(client, containerId, "eltororojo_itemdb-network");
}

async Task CreateConsumer(IDockerClient client, string fileName)
{
    const string imageName = "incoming.consumer";
    const string dockerFilePath = "/builds/Incoming.Consumer";
    await BuildImage(client, imageName, dockerFilePath);

    var parameters = new CreateContainerParameters
    {
        Name = $"incoming.consumer-{fileName}",
        Image = imageName,
        Hostname = $"incoming.consumer-{fileName}",
        HostConfig = new HostConfig
        { 
            AutoRemove = true,
            NetworkMode = "eltororojo_incoming-queue-network"
        },
        Env = new List<string>
        {
            $"FILE_NAME={fileName}"
        }
    };

    var containerId = await CreateContainer(client, parameters, "Consumer", fileName);

    await ConnectToNetwork(client, containerId, "eltororojo_incoming-api-network");
    await ConnectToNetwork(client, containerId, "eltororojo_errordb-network");
}

async Task CreateProducer(IDockerClient client, string fileName)
{
    const string imageName = "incoming.producer";
    const string dockerFilePath = "/builds/Incoming.Producer";
    await BuildImage(client, imageName, dockerFilePath);

    var parameters = new CreateContainerParameters
    {
        Name = $"incoming.producer-{fileName}",
        Image = imageName,
        Hostname = $"incoming.producer-{fileName}",
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
            $"FILE_PATH={Path.Combine(workspaceDir, fileName)}"
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

async Task ConnectToNetwork(IDockerClient client, string containerId, string networkName)
{
    var parameters = new NetworkConnectParameters
    {
        Container = containerId
    };

    await client.Networks.ConnectNetworkAsync(networkName, parameters);
    Console.WriteLine($"Connected container {containerId} to network: {networkName} {DateTime.Now.ToLongTimeString()}");
}

async Task Cleanup(IDockerClient client, string targetPurpose, ICollection<string> dependentPurposes)
{
    var listParameters = new ContainersListParameters { All = true };

    Console.WriteLine($"Waiting for dependent ({string.Join(", ", dependentPurposes)}) containers to be removed. {DateTime.Now.ToLongTimeString()}");

    var dependentContainersExists = true;

    while (dependentContainersExists)
    {
        var dependentContainers = await client.Containers.ListContainersAsync(listParameters);
        dependentContainersExists = dependentContainers
            .Any(container => dependentPurposes.Contains(container.Labels["purpose"]));

        if (dependentContainersExists)
            Thread.Sleep(TimeSpan.FromSeconds(1));
    }

    Console.WriteLine($"Removing {targetPurpose} containers. {DateTime.Now.ToLongTimeString()}");

    var stopParameters = new ContainerStopParameters { WaitBeforeKillSeconds = 10 };

    var targetContainers = await client.Containers.ListContainersAsync(listParameters);
    var targetContainerIds = targetContainers
        .Where(container => container.Labels["purpose"].Equals(targetPurpose))
        .Select(container => container.ID);

    foreach (var targetContainerId in targetContainerIds)
    {
        await client.Containers.StopContainerAsync(targetContainerId, stopParameters);
        Console.WriteLine($"Stopped (and automatically removed) {targetPurpose} container: {targetContainerId}. {DateTime.Now.ToLongTimeString()}");
    }
}

async Task PullImage(IDockerClient client, string imageName, string imageTag = "latest")
{
    var imageAlreadyExists = await ImageAlreadyExists(client, imageName, imageTag);
    if (!imageAlreadyExists)
    {
        Console.WriteLine($"Image named '{imageName}:{imageTag}' not found. Pulling image. {DateTime.Now.ToLongTimeString()}");

        var imagesCreateParameters = new ImagesCreateParameters
        {
            FromImage = imageName,
            Tag = imageTag
        };

        var progress = new Progress<JSONMessage>();
        progress.ProgressChanged += (sender, e) =>
        {
            Console.WriteLine($"From: {e.From}");
            Console.WriteLine($"Status: {e.Status}");
            Console.WriteLine($"Stream: {e.Stream}");
            Console.WriteLine($"ID: {e.ID}");
            Console.WriteLine($"Progress: {e.ProgressMessage}");
            Console.WriteLine($"Error: {e.ErrorMessage}");
        };

        await client.Images.CreateImageAsync(imagesCreateParameters, new AuthConfig(), progress);

        Console.WriteLine($"Image named '{imageName}' pulled. {DateTime.Now.ToLongTimeString()}");
    }
}

async Task BuildImage(IDockerClient client, string imageName, string dockerFilePath, bool rebuild = false)
{
    if (!rebuild)
        rebuild = !await ImageAlreadyExists(client, imageName); //If should not rebuild existing image, check if it exists. If yes then do not build.

    if (rebuild)
    {
        Console.WriteLine($"Image named '{imageName}' not found. Building image from {dockerFilePath}. {DateTime.Now.ToLongTimeString()}");

        var imageBuildParameters = new ImageBuildParameters
        {
            Tags = new List<string> { imageName }
        };

        var progress = new Progress<JSONMessage>();
        progress.ProgressChanged += (sender, e) =>
        {
            Console.WriteLine($"From: {e.From}");
            Console.WriteLine($"Status: {e.Status}");
            Console.WriteLine($"Stream: {e.Stream}");
            Console.WriteLine($"ID: {e.ID}");
            Console.WriteLine($"Progress: {e.ProgressMessage}");
            Console.WriteLine($"Error: {e.ErrorMessage}");
        };

        await using var stream = CreateTarFileForDockerfileDirectory(dockerFilePath);
        await client.Images.BuildImageFromDockerfileAsync(imageBuildParameters, stream, null, null, progress);

        Console.WriteLine($"Image named '{imageName}' built. {DateTime.Now.ToLongTimeString()}");
    }
}

async Task<bool> ImageAlreadyExists(IDockerClient client, string imageName, string imageTag = "latest")
{
    var listImagesParameters = new ImagesListParameters { All = true };
    var images = await client.Images.ListImagesAsync(listImagesParameters);
    return images.Any(image => image.RepoTags.Contains($"{imageName}:{imageTag}"));
}

Stream CreateTarFileForDockerfileDirectory(string directory)
{
    var stream = new MemoryStream();
    var filePaths = Directory.GetFiles(directory, "*.*", SearchOption.AllDirectories);

    using var archive = new TarOutputStream(stream, Encoding.UTF8);

    //Prevent the TarOutputStream from closing the underlying memory stream when done
    archive.IsStreamOwner = false;

    //Add files to tar archive
    foreach (var file in filePaths)
    {
        var tarName = Path.GetFileName(file);
        
        var entry = TarEntry.CreateTarEntry(tarName);
        using var fileStream = File.OpenRead(file);
        entry.Size = fileStream.Length;
        archive.PutNextEntry(entry);
        
        var localBuffer = new byte[32 * 1024];

        while (true)
        {
            var numberOfBytesSavedToBuffer = fileStream.Read(localBuffer, 0, localBuffer.Length);
            if (numberOfBytesSavedToBuffer <= 0)
                break;

            archive.Write(localBuffer, 0, numberOfBytesSavedToBuffer);
        }
        archive.CloseEntry();
    }

    archive.Close();
    
    stream.Position = 0;
    return stream;
}
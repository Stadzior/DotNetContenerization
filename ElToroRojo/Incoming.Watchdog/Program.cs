using Docker.DotNet;
using Docker.DotNet.Models;

while(true)
{
    const string workspaceDir = "/workspace";
    var files = Directory.GetFiles(workspaceDir);

    if (files.Any())
    {
        var dockerEngineUrl = new Uri("unix://var/run/docker.sock");
        using var client = new DockerClientConfiguration(dockerEngineUrl)
            .CreateClient();
        
        foreach (var filePath in files)
        {
            var fileName = Path.GetFileName(filePath);
            
            var createParameters = new CreateContainerParameters
            {
                Name = $"incoming.producer-{fileName}",
                Image = "incoming.producer:latest",
                HostConfig = new HostConfig
                {
                    Binds = new List<string>
                    {
                        @"C:\workspace:/workspace:rw"
                    }
                },
                Env = new List<string> { $"FILE_PATH={filePath}" }
            };

            var createResponse = await client.Containers.CreateContainerAsync(createParameters);
            
            Console.WriteLine($"Container with id: {createResponse.ID}, created for file: {fileName}");

            await client.Containers.StartContainerAsync(createResponse.ID, new ContainerStartParameters());

            Console.WriteLine($"Started container with id: {createResponse.ID}");

            Task.Factory.StartNew(() =>
            {
                client.Containers.WaitContainerAsync(createResponse.ID);

                var removeParameters = new ContainerRemoveParameters
                {
                    Force = true,
                    RemoveLinks = true,
                    RemoveVolumes = true
                };

                client.Containers.RemoveContainerAsync(createResponse.ID, removeParameters);
            });
        }
    }
}
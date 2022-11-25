using Docker.DotNet;
using Docker.DotNet.Models;

while(true)
{
    const string workspaceDir = "/workspace";
    var files = Directory.GetFiles(workspaceDir);
    foreach (var filePath in files)
    {
        var fileName = Path.GetFileName(filePath);
        Console.WriteLine($"Creating container for file: {fileName}");
        
        var dockerEngineUrl = new Uri("/var/run/docker.sock");
        using var client = new DockerClientConfiguration(dockerEngineUrl)
             .CreateClient();
        
        var createParameters = new CreateContainerParameters 
        {
            Name = $"incoming.producer-{fileName}",
            Image = "incoming.producer:latest",
            Env = new List<string> {$"FILE_PATH={filePath}"}
        };

        var createResponse = await client.Containers.CreateContainerAsync(createParameters);
    
        await client.Containers.StartContainerAsync(createResponse.ID, new ContainerStartParameters());
    }
}
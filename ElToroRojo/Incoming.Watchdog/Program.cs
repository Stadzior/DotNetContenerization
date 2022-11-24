using Docker.DotNet;
using Docker.DotNet.Models;

while(true)
{
    const string workspaceDir = @"C:\workspace";
    var files = Directory.GetFiles(workspaceDir);
    foreach (var filePath in files)
    {
        var fileName = Path.GetFileName(filePath);
        Console.WriteLine($"Creating container for file: {fileName}");
        File.Delete(filePath);
        
        var dockerEngineUrl = new Uri("npipe://./pipe/docker_engine");
        using var client = new DockerClientConfiguration(dockerEngineUrl)
             .CreateClient();
        
        var createParameters = new CreateContainerParameters 
        {
            Name = $"Image.Producer-{fileName}",
            Image = "Image.Producer:latest"
        };

        var createResponse = await client.Containers.CreateContainerAsync(createParameters);
        
        await client.Containers.StartContainerAsync(createResponse.ID, new ContainerStartParameters());
    }
}
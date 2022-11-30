var filePath = args[0];
Console.WriteLine($"Processing file: {filePath}");

if (!File.Exists(filePath))
{
    File.Delete(filePath);
    Console.WriteLine("File deleted.");
}
else
    Console.WriteLine("File does not exist.");
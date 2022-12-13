using LegacyModule;

while(true)
{
    var data = new LegacyClass().GetData();
    Console.WriteLine(data);
    await Task.Delay(100);
}

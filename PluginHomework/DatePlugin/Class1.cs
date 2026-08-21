using PluginBase;

namespace DatePlugin;

public class CurrentDatePlugin : IPlugin
{
    public string Name => "Current Date Plugin";

    public void Execute()
    {
        Console.WriteLine(DateTime.Now);
    }
}
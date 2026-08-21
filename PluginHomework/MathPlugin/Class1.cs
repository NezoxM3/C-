using PluginBase;

namespace MathPlugin;

public class MathPlugin : IPlugin
{
    public string Name => "Math Plugin";

    public void Execute()
    {
        int a = 2;
        int b = 3;
        int result = a + b;

        Console.WriteLine($"{a} + {b} = {result}");
    }
}
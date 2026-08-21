using PluginBase;

namespace RandomPlugin;

public class RandomNumberPlugin : IPlugin
{
    public string Name => "Random Number Plugin";

    public void Execute()
    {
        Random random = new Random();

        int number = random.Next(1, 1000);

        Console.WriteLine(number);
    }
}
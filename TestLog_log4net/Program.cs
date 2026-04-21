using log4net;
using log4net.Config;
using System.Reflection;

class Program
{
    private static readonly ILog log = LogManager.GetLogger(typeof(Program));

    static void Main(string[] args)
    {
        var repo = LogManager.GetRepository(Assembly.GetEntryAssembly());
        XmlConfigurator.Configure(repo, new System.IO.FileInfo("log4net.config"));

        int input = 10;
        log.Info($"Вхідні дані: {input}");

        int result = input * 2;
        log.Info($"Результат: {result}");

        System.Console.WriteLine($"Result: {result}");
    }
}
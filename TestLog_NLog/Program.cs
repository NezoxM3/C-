using NLog;

class Program
{
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    static void Main(string[] args)
    {
        int input = 10;
        logger.Info($"Вхідні дані: {input}");

        int result = input * 2;
        logger.Info($"Результат: {result}");

        System.Console.WriteLine($"Result: {result}");
    }
}
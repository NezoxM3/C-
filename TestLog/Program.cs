using Serilog;

class Program
{
    static void Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.File("logs/log.txt")
            .CreateLogger();

        int input = 10;
        Log.Information("Вхідні дані: {Input}", input);

        int result = input * 2;
        Log.Information("Результат: {Result}", result);

        Console.WriteLine($"Result: {result}");

        Log.CloseAndFlush();
    }
}
using System.Threading;

class Program
{
    private const string MutexName = "MyMutex";

    static void Main()
    {
        using Mutex mutex = new Mutex(false, MutexName);

        Console.WriteLine("===== MUTEX CONTROLLER =====");
        Console.WriteLine();
        Console.WriteLine("1 - Release Mutex");
        Console.WriteLine("2 - Lock Mutex");
        Console.WriteLine("0 - Exit");
        Console.WriteLine();

        while (true)
        {
            ConsoleKeyInfo key = Console.ReadKey(true);

            if (key.KeyChar == '1')
            {
                try
                {
                    mutex.ReleaseMutex();
                    Console.WriteLine("Mutex released.");
                }
                catch (ApplicationException)
                {
                    Console.WriteLine(
                        "Mutex is not owned by this process.");
                }
            }
            else if (key.KeyChar == '2')
            {
                try
                {
                    mutex.WaitOne();
                    Console.WriteLine("Mutex locked.");
                }
                catch (AbandonedMutexException)
                {
                    Console.WriteLine("Mutex was abandoned. Mutex locked.");
                }
            }
            else if (key.KeyChar == '0')
            {
                break;
            }
        }
    }
}
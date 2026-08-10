using System.Threading;

class Program
{
    private const string MutexName = "MyMutex";

    static void Main()
    {
        using Mutex mutex = new Mutex(false, MutexName);

        Console.WriteLine("===== MUTEX WORKER =====");
        Console.WriteLine("Worker started.");
        Console.WriteLine();

        while (true)
        {
            bool hasAccess = false;

            try
            {
                hasAccess = mutex.WaitOne(0);

                if (hasAccess)
                {
                    Console.WriteLine("Working...");
                    Thread.Sleep(1000);

                    mutex.ReleaseMutex();
                    hasAccess = false;
                }
                else
                {
                    Thread.Sleep(100);
                }
            }
            catch (AbandonedMutexException)
            {
                Console.WriteLine("Mutex was abandoned.");
            }
        }
    }
}
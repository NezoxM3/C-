using System.Threading;

class Program
{
    static Semaphore semaphore = new Semaphore(3, 3);

    static void Main()
    {
        Thread[] threads = new Thread[10];

        for (int i = 0; i < 10; i++)
        {
            int threadNumber = i + 1;

            threads[i] = new Thread(() =>
            {
                Console.WriteLine($"Thread {threadNumber} is waiting");

                semaphore.WaitOne();

                try
                {
                    Console.WriteLine($"Thread {threadNumber} started working");

                    Thread.Sleep(2000);

                    Console.WriteLine($"Thread {threadNumber} finished working");
                }
                finally
                {
                    semaphore.Release();
                }
            });

            threads[i].Start();
        }

        foreach (Thread thread in threads)
        {
            thread.Join();
        }

        Console.WriteLine("\nAll threads have finished.");
    }
}
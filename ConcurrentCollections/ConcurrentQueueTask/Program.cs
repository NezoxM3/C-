using System;
using System.Collections.Concurrent;
using System.Threading;

class Program
{
    static void Main()
    {
        var queue = new ConcurrentQueue<int>();

        using CancellationTokenSource cts =
            new CancellationTokenSource(TimeSpan.FromSeconds(15));

        CancellationToken token = cts.Token;

        Thread[] producers = new Thread[3];

        for (int i = 0; i < 3; i++)
        {
            int threadNumber = i + 1;

            producers[i] = new Thread(() =>
            {
                Random random = new Random(
                    Environment.TickCount + threadNumber);

                while (!token.IsCancellationRequested)
                {
                    int number = random.Next(1, 101);

                    queue.Enqueue(number);

                    Thread.Sleep(200);
                }
            });

            producers[i].Start();
        }

        Thread consumer = new Thread(() =>
        {
            while (!token.IsCancellationRequested || !queue.IsEmpty)
            {
                if (queue.TryDequeue(out int number))
                {
                    Console.WriteLine($"Read from queue: {number}");
                }
                else
                {
                    Thread.Sleep(50);
                }
            }
        });

        consumer.Start();

        Thread.Sleep(15000);

        cts.Cancel();

        foreach (Thread producer in producers)
        {
            producer.Join();
        }

        consumer.Join();

        Console.WriteLine();
        Console.WriteLine("Program finished after 15 seconds.");
    }
}
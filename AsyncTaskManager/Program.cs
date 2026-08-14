using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

class Program
{

    private static readonly ManualResetEventSlim PauseEvent =
        new ManualResetEventSlim(true);

    static async Task Main()
    {
        Console.WriteLine("=== Async Task Manager ===");
        Console.WriteLine();
        Console.WriteLine("P - Pause");
        Console.WriteLine("R - Resume");
        Console.WriteLine("ESC - Stop all tasks");
        Console.WriteLine();

        using CancellationTokenSource cancellationTokenSource =
            new CancellationTokenSource();

        CancellationToken cancellationToken =
            cancellationTokenSource.Token;

        // Start 2 main tasks
        Task task1 = RunMainTaskAsync(1, cancellationToken);
        Task task2 = RunMainTaskAsync(2, cancellationToken);

        // Listen for keyboard commands
        while (!cancellationToken.IsCancellationRequested)
        {
            ConsoleKeyInfo key = Console.ReadKey(true);

            if (key.Key == ConsoleKey.Escape)
            {
                Console.WriteLine();
                Console.WriteLine("ESC pressed. Stopping all tasks...");

                cancellationTokenSource.Cancel();

                PauseEvent.Set();

                break;
            }

            if (key.Key == ConsoleKey.P)
            {
                PauseEvent.Reset();

                Console.WriteLine("All subtasks paused.");
            }

            if (key.Key == ConsoleKey.R)
            {
                PauseEvent.Set();

                Console.WriteLine("All subtasks resumed.");
            }
        }

        await Task.WhenAll(task1, task2);

        Console.WriteLine();
        Console.WriteLine("All tasks have finished.");
    }



    static async Task RunMainTaskAsync(
        int taskNumber,
        CancellationToken cancellationToken)
    {
        var subtasks = new List<Task>();

        for (int i = 1; i <= 5; i++)
        {
            int subTaskNumber = i;

            subtasks.Add(
                RunSubTaskAsync(
                    taskNumber,
                    subTaskNumber,
                    cancellationToken)
            );
        }

        await Task.WhenAll(subtasks);
    }


    static async Task RunSubTaskAsync(
        int taskNumber,
        int subTaskNumber,
        CancellationToken cancellationToken)
    {
        try
        {
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                PauseEvent.Wait(cancellationToken);

                Console.WriteLine(
                    $"Task {taskNumber} -> SubTask {subTaskNumber} : Працює...");

                await Task.Delay(1000, cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine(
                $"Task {taskNumber} -> SubTask {subTaskNumber} : Успішне переривання роботи...");
        }
    }
}
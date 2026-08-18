using System;
using System.Diagnostics;
using System.Linq;

class Program
{
    const int DataSize = 10_000_000;

    static void Main()
    {
        Console.WriteLine("==========================================");
        Console.WriteLine("       PLINQ LARGE ARRAY ANALYSIS");
        Console.WriteLine("==========================================");
        Console.WriteLine();
        Console.WriteLine($"Generating {DataSize:N0} random numbers...");

        Random random = new Random(42);

        int[] numbers = new int[DataSize];

        for (int i = 0; i < numbers.Length; i++)
        {
            numbers[i] = random.Next(1, 1_000_001);
        }

        Console.WriteLine("Data generated.");
        Console.WriteLine();


        Console.WriteLine("========== STANDARD LINQ ==========");

        Stopwatch stopwatch = Stopwatch.StartNew();

        int[] linqResult = numbers
            .Where(x => x % 2 == 0)
            .Select(x => x * x)
            .OrderBy(x => x)
            .ToArray();

        stopwatch.Stop();

        Console.WriteLine(
            $"LINQ time: {stopwatch.ElapsedMilliseconds} ms");

        Console.WriteLine(
            $"LINQ elements: {linqResult.Length}");

        Console.WriteLine();



        Console.WriteLine("========== PLINQ ==========");

        stopwatch.Restart();

        int[] plinqResult = numbers
            .AsParallel()
            .Where(x => x % 2 == 0)
            .Select(x => x * x)
            .OrderBy(x => x)
            .ToArray();

        stopwatch.Stop();

        Console.WriteLine(
            $"PLINQ time: {stopwatch.ElapsedMilliseconds} ms");

        Console.WriteLine(
            $"PLINQ elements: {plinqResult.Length}");

        Console.WriteLine();


        Console.WriteLine("========== COMPARISON ==========");

        bool sameResult =
            linqResult.SequenceEqual(plinqResult);

        Console.WriteLine(
            $"Results are equal: {sameResult}");

        Console.WriteLine();


        Console.WriteLine("========== DEGREE OF PARALLELISM ==========");

        int[] degrees = { 1, 2, 4, 8 };

        foreach (int degree in degrees)
        {
            stopwatch.Restart();

            int[] result = numbers
                .AsParallel()
                .WithDegreeOfParallelism(degree)
                .Where(x => x % 2 == 0)
                .Select(x => x * x)
                .OrderBy(x => x)
                .ToArray();

            stopwatch.Stop();

            Console.WriteLine(
                $"Degree {degree}: {stopwatch.ElapsedMilliseconds} ms | " +
                $"Elements: {result.Length}");
        }

        Console.WriteLine();


        Console.WriteLine("========== ORDERING ==========");

        stopwatch.Restart();

        int[] unorderedResult = numbers
            .AsParallel()
            .Where(x => x % 2 == 0)
            .Select(x => x * x)
            .ToArray();

        stopwatch.Stop();

        Console.WriteLine(
            $"Without AsOrdered(): {stopwatch.ElapsedMilliseconds} ms");

        Console.WriteLine(
            "First 10 elements:");

        PrintFirstElements(unorderedResult);


        stopwatch.Restart();

        int[] orderedResult = numbers
            .AsParallel()
            .AsOrdered()
            .Where(x => x % 2 == 0)
            .Select(x => x * x)
            .ToArray();

        stopwatch.Stop();

        Console.WriteLine(
            $"With AsOrdered(): {stopwatch.ElapsedMilliseconds} ms");

        Console.WriteLine(
            "First 10 elements:");

        PrintFirstElements(orderedResult);

        Console.WriteLine();


        Console.WriteLine("========== MERGE OPTIONS ==========");

        RunMergeTest(
            "NotBuffered",
            ParallelMergeOptions.NotBuffered,
            numbers);

        RunMergeTest(
            "AutoBuffered",
            ParallelMergeOptions.AutoBuffered,
            numbers);

        RunMergeTest(
            "FullyBuffered",
            ParallelMergeOptions.FullyBuffered,
            numbers);

        Console.WriteLine();


        Console.WriteLine("========== FORALL() ==========");

        Stopwatch forAllStopwatch = Stopwatch.StartNew();

        long count = 0;

        numbers
            .AsParallel()
            .Where(x => x % 2 == 0)
            .ForAll(x =>
            {
                Interlocked.Increment(ref count);
            });

        forAllStopwatch.Stop();

        Console.WriteLine(
            $"ForAll() time: {forAllStopwatch.ElapsedMilliseconds} ms");

        Console.WriteLine(
            $"ForAll() elements: {count}");

        Console.WriteLine();


        Console.WriteLine("==========================================");
        Console.WriteLine("Analysis completed.");
        Console.WriteLine("==========================================");
    }

    static void PrintFirstElements(int[] values)
    {
        int count = Math.Min(10, values.Length);

        for (int i = 0; i < count; i++)
        {
            Console.Write(values[i]);

            if (i < count - 1)
            {
                Console.Write(", ");
            }
        }

        Console.WriteLine();
    }


    static void RunMergeTest(
        string name,
        ParallelMergeOptions mergeOption,
        int[] numbers)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();

        int count = 0;

        numbers
            .AsParallel()
            .WithMergeOptions(mergeOption)
            .Where(x => x % 2 == 0)
            .Select(x => x * x)
            .ForAll(x =>
            {
                Interlocked.Increment(ref count);
            });

        stopwatch.Stop();

        Console.WriteLine(
            $"{name}: {stopwatch.ElapsedMilliseconds} ms | " +
            $"Elements: {count}");
    }
}
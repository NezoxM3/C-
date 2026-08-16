using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading;

class Program
{
    static void Main()
    {
        string wordsFile = "words.txt";
        string textFile = "text.txt";

        if (!File.Exists(wordsFile))
        {
            Console.WriteLine("words.txt was not found.");
            return;
        }

        if (!File.Exists(textFile))
        {
            Console.WriteLine("text.txt was not found.");
            return;
        }

        string[] words = File.ReadAllLines(wordsFile)
            .Where(word => !string.IsNullOrWhiteSpace(word))
            .Select(word => word.Trim().ToLower())
            .ToArray();

        string text = File.ReadAllText(textFile).ToLower();

        char[] separators =
        {
            ' ', '\n', '\r', '\t',
            '.', ',', '!', '?',
            ':', ';', '(', ')'
        };

        string[] textWords = text
            .Split(separators, StringSplitOptions.RemoveEmptyEntries);

        var statistics =
            new ConcurrentDictionary<string, int>();

        foreach (string word in words)
        {
            statistics.TryAdd(word, 0);
        }

        Thread[] threads = new Thread[5];

        for (int i = 0; i < 5; i++)
        {
            int threadNumber = i + 1;

            threads[i] = new Thread(() =>
            {
                Random random = new Random(
                    Environment.TickCount + threadNumber);

                for (int j = 0; j < 100; j++)
                {
                    string selectedWord =
                        words[random.Next(words.Length)];

                    int occurrences = textWords.Count(
                        word => word == selectedWord);

                    if (occurrences > 0)
                    {
                        statistics.AddOrUpdate(
                            selectedWord,
                            occurrences,
                            (key, oldValue) =>
                                oldValue + occurrences);
                    }

                    Thread.Sleep(10);
                }
            });

            threads[i].Start();
        }

        foreach (Thread thread in threads)
        {
            thread.Join();
        }

        Console.WriteLine("===== WORD STATISTICS =====");

        foreach (var item in statistics.OrderByDescending(x => x.Value))
        {
            Console.WriteLine(
                $"{item.Key}: {item.Value}");
        }

        Console.WriteLine();
        Console.WriteLine("All threads finished.");
    }
}
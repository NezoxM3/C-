using System;
using System.Collections.Concurrent;
using System.Threading;

class Program
{
    static void Main()
    {
        var dishes = new BlockingCollection<string>();

        string[] menu =
        {
            "Borscht",
            "Pizza",
            "Salad",
            "Sushi"
        };

        const int totalDishes = 20;

        Thread cook = new Thread(() =>
        {
            Random random = new Random();

            for (int i = 1; i <= totalDishes; i++)
            {
                string dish = menu[random.Next(menu.Length)];

                Thread.Sleep(500);

                dishes.Add(dish);

                Console.WriteLine(
                    $"Cook prepared: {dish} ({i}/{totalDishes})");
            }

            dishes.CompleteAdding();
        });

        Thread waiter = new Thread(() =>
        {
            foreach (string dish in dishes.GetConsumingEnumerable())
            {
                Console.WriteLine(
                    $"Waiter served: {dish}");

                Thread.Sleep(700);
            }

            Console.WriteLine("Waiter finished working.");
        });

        cook.Start();
        waiter.Start();

        cook.Join();
        waiter.Join();

        Console.WriteLine();
        Console.WriteLine("20 dishes were prepared.");
        Console.WriteLine("Program finished.");
    }
}
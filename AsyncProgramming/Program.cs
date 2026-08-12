using System;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        Console.WriteLine("===== TASK 1: DATA LOADING =====");

        Task usersTask = LoadUsersAsync();
        Task productsTask = LoadProductsAsync();

        await Task.WhenAll(usersTask, productsTask);

        Console.WriteLine("Data loaded.");
        Console.WriteLine();


        Console.WriteLine("===== TASK 2: PROCESSING RESULTS =====");

        Task<int> numberTask1 = GetNumberAsync(2, 1000);
        Task<int> numberTask2 = GetNumberAsync(3, 2000);
        Task<int> numberTask3 = GetNumberAsync(5, 3000);

        int[] results = await Task.WhenAll(
            numberTask1,
            numberTask2,
            numberTask3
        );

        int sum = results.Sum();

        Console.WriteLine($"Result 1: {results[0]}");
        Console.WriteLine($"Result 2: {results[1]}");
        Console.WriteLine($"Result 3: {results[2]}");
        Console.WriteLine($"Sum: {sum}");
        Console.WriteLine();


        Console.WriteLine("===== TASK 3: ERROR HANDLING =====");

        try
        {
            string result = await ProcessDataAsync();

            Console.WriteLine(result);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }

        Console.WriteLine();
        Console.WriteLine("Program finished.");
    }


    // 1

    static async Task LoadUsersAsync()
    {
        Console.WriteLine("Loading users...");

        await Task.Delay(2000);

        Console.WriteLine("Users loaded.");
    }


    static async Task LoadProductsAsync()
    {
        Console.WriteLine("Loading products...");

        await Task.Delay(3000);

        Console.WriteLine("Products loaded.");
    }


    // 2

    static async Task<int> GetNumberAsync(int number, int delay)
    {
        await Task.Delay(delay);

        return number;
    }


    // 3

    static async Task<string> ProcessDataAsync()
    {
        await Task.Delay(1000);

        Random random = new Random();

        if (random.Next(2) == 0)
        {
            throw new Exception("Something went wrong while processing data.");
        }

        return "Operation completed successfully.";
    }
}
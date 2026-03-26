using System;
using System.IO;
using System.Linq;

class Program
{
    static void Main()
    {
        while (true)
        {
            Console.WriteLine("\nОберіть завдання:");
            Console.WriteLine("1 - Генерація чисел");
            Console.WriteLine("2 - Пошук і заміна");
            Console.WriteLine("3 - Модератор");
            Console.WriteLine("4 - Реверс файлу");
            Console.WriteLine("5 - Статистика чисел");
            Console.WriteLine("6 - Пошук файлів");
            Console.WriteLine("7 - Видалення файлів");
            Console.WriteLine("0 - Вихід");

            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1": Task1(); break;
                case "2": Task2(); break;
                case "3": Task3(); break;
                case "4": Task4(); break;
                case "5": Task5(); break;
                case "6": Task6(); break;
                case "7": Task7(); break;
                case "0": return;
            }
        }
    }

    // Завдання 1
    static void Task1()
    {
        Random rnd = new Random();
        int[] numbers = Enumerable.Range(0, 100)
            .Select(_ => rnd.Next(0, 1000)).ToArray();

        bool IsPrime(int n)
        {
            if (n < 2) return false;
            for (int i = 2; i <= Math.Sqrt(n); i++)
                if (n % i == 0) return false;
            return true;
        }

        bool IsFibonacci(int n)
        {
            int a = 0, b = 1;
            while (b < n)
            {
                int temp = a + b;
                a = b;
                b = temp;
            }
            return b == n || n == 0;
        }

        var primes = numbers.Where(IsPrime);
        var fibs = numbers.Where(IsFibonacci);

        File.WriteAllLines("primes.txt", primes.Select(x => x.ToString()));
        File.WriteAllLines("fibonacci.txt", fibs.Select(x => x.ToString()));

        Console.WriteLine($"Всього: {numbers.Length}");
        Console.WriteLine($"Прості: {primes.Count()}");
        Console.WriteLine($"Фібоначчі: {fibs.Count()}");
    }

    // Завдання 2
    static void Task2()
    {
        Console.Write("Файл: ");
        string path = Console.ReadLine();

        Console.Write("Що шукати: ");
        string search = Console.ReadLine();

        Console.Write("На що замінити: ");
        string replace = Console.ReadLine();

        string text = File.ReadAllText(path);
        int count = text.Split(search).Length - 1;

        text = text.Replace(search, replace);
        File.WriteAllText(path, text);

        Console.WriteLine($"Замінено: {count}");
    }

    // Завдання 3
    static void Task3()
    {
        Console.Write("Файл тексту: ");
        string textPath = Console.ReadLine();

        Console.Write("Файл слів: ");
        string badWordsPath = Console.ReadLine();

        string text = File.ReadAllText(textPath);
        string[] badWords = File.ReadAllText(badWordsPath)
            .Split(new[] { ' ', '.', ',', '\n' }, StringSplitOptions.RemoveEmptyEntries);

        int count = 0;

        foreach (var word in badWords)
        {
            if (text.Contains(word))
            {
                text = text.Replace(word, new string('*', word.Length));
                count++;
            }
        }

        File.WriteAllText(textPath, text);

        Console.WriteLine($"Замінено слів: {count}");
    }

    // Завдання 4
    static void Task4()
    {
        Console.Write("Файл: ");
        string path = Console.ReadLine();

        string text = File.ReadAllText(path);
        char[] arr = text.ToCharArray();
        Array.Reverse(arr);

        File.WriteAllText("reversed.txt", new string(arr));

        Console.WriteLine("Готово.");
    }

    // Завдання 5
    static void Task5()
    {
        Console.Write("Файл з числами: ");
        string path = Console.ReadLine();

        int[] nums = File.ReadAllLines(path).Select(int.Parse).ToArray();

        var positive = nums.Where(x => x > 0);
        var negative = nums.Where(x => x < 0);
        var twoDigit = nums.Where(x => Math.Abs(x) >= 10 && Math.Abs(x) <= 99);
        var fiveDigit = nums.Where(x => Math.Abs(x) >= 10000 && Math.Abs(x) <= 99999);

        File.WriteAllLines("positive.txt", positive.Select(x => x.ToString()));
        File.WriteAllLines("negative.txt", negative.Select(x => x.ToString()));

        Console.WriteLine($"Додатні: {positive.Count()}");
        Console.WriteLine($"Від'ємні: {negative.Count()}");
        Console.WriteLine($"2-значні: {twoDigit.Count()}");
        Console.WriteLine($"5-значні: {fiveDigit.Count()}");
    }

    // Завдання 6
    static void Task6()
    {
        Console.Write("Шлях: ");
        string path = Console.ReadLine();

        Console.Write("Маска: ");
        string mask = Console.ReadLine();

        var files = Directory.GetFiles(path, mask, SearchOption.AllDirectories);

        foreach (var file in files)
            Console.WriteLine(file);

        Console.WriteLine($"Знайдено: {files.Length}");
    }

    // Завдання 7
    static void Task7()
    {
        Console.Write("Шлях: ");
        string path = Console.ReadLine();

        Console.Write("Маска: ");
        string mask = Console.ReadLine();

        var files = Directory.GetFiles(path, mask, SearchOption.AllDirectories);

        foreach (var file in files)
        {
            File.Delete(file);
            Console.WriteLine($"Видалено: {file}");
        }

        Console.WriteLine($"Всього: {files.Length}");
    }
}
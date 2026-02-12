// 1. Створіть клас "Місто". Необхідно зберігати у полях класу: назву міста, 
// назву країни, кількість жителів у місті, телефонний код міста, назву районів міста. 
// Реалізуйте методи класу для введення даних, виведення даних. 
// Реалізуйте доступ до окремих полів через методи класу або властивості.

using System;
using System.Collections.Generic;

namespace HomeworkAllTasks
{
// 1. Створіть клас "Місто". Необхідно зберігати у полях класу: назву міста, 
// назву країни, кількість жителів у місті, телефонний код міста, назву районів міста. 
// Реалізуйте методи класу для введення даних, виведення даних. 
// Реалізуйте доступ до окремих полів через методи класу або властивості.
    class City
    {
        public string Name { get; set; }
        public string Country { get; set; }
        public int Population { get; set; }
        public string PhoneCode { get; set; }
        public List<string> Districts { get; set; }

        public City()
        {
            Districts = new List<string>();
        }

        public void InputData()
        {
            Console.Write("Назва міста: ");
            Name = Console.ReadLine();

            Console.Write("Країна: ");
            Country = Console.ReadLine();

            Console.Write("Кількість жителів: ");
            Population = int.Parse(Console.ReadLine());

            Console.Write("Телефонний код: ");
            PhoneCode = Console.ReadLine();

            Console.Write("Кількість районів: ");
            int count = int.Parse(Console.ReadLine());

            for (int i = 0; i < count; i++)
            {
                Console.Write($"Район {i + 1}: ");
                Districts.Add(Console.ReadLine());
            }
        }

        public void PrintData()
        {
            Console.WriteLine("\n--- Інформація про місто ---");
            Console.WriteLine($"Місто: {Name}");
            Console.WriteLine($"Країна: {Country}");
            Console.WriteLine($"Населення: {Population}");
            Console.WriteLine($"Телефонний код: {PhoneCode}");
            Console.WriteLine("Райони:");
            foreach (var d in Districts)
                Console.WriteLine($"- {d}");
        }
    }

// 2.
// Створіть класс з методом, що фільтрує масив на підставі переданих параметрів. Метод приймає параметри: оригінальний_масив, масив_з_даними_для_фільтрації.
// Метод повертає оригінальний масив без елементів, які є в масиві для фільтрації.
// Наприклад:
// 1 2 6 -1 88 7 6 — оригінальний масив;
// 6 88 7 — масив для фільтрації;
// 1 2 -1 — результат роботи методу.

    class ArrayFilter
    {
        public static int[] Filter(int[] original, int[] filter)
        {
            List<int> result = new List<int>();

            foreach (int item in original)
            {
                if (!Array.Exists(filter, f => f == item))
                    result.Add(item);
            }

            return result.ToArray();
        }
    }

// 3.
// Класс, з методом, який відображає квадрат з деякого символу. 
// Наприклад 5, + - малюємо квадрат стороною 5 з символу +
// Необхідні параметри передавати через конструктор або властивості.
// Напишіть код який отримує дані від користувача і використовує їх для конфігування вашого класу

    class SquareDrawer
    {
        public int Size { get; set; }
        public char Symbol { get; set; }

        public SquareDrawer(int size, char symbol)
        {
            Size = size;
            Symbol = symbol;
        }

        public void Draw()
        {
            Console.WriteLine("\n--- Квадрат ---");
            for (int i = 0; i < Size; i++)
            {
                for (int j = 0; j < Size; j++)
                    Console.Write(Symbol + " ");
                Console.WriteLine();
            }
        }
    }

// 4.
// Напишіть класс, який приймає число  (через конструктор або свойство) та має матоди або свойства які:

// - повертає чи число паліндромом bool Palindrom {get {...}}

// Паліндром — число, яке читається однаково як справа наліво, так і зліва направо.
// Наприклад:
// 1221 — паліндром;
// 3443 — паліндром;
// 7854 — не паліндром.

// - повертає число з переставленими цифрами навпаки
// - метод який приймає число та повертає скількі разів ця цифра зустрічається в ньому
// - чи є парним (Even)
// - чи є непарним (Odd)
// - чи є відємним (Negative)
// - чи є простим (Simple) 

// Просте число – це натуральне число, яке має рівно два різні дільники: одиницю і саме себе. Наприклад, 2, 3, 5, 7, 11 є простими числами, оскільки вони діляться без остачі тільки на 1 та на самих себе.


    class NumberAnalyzer
    {
        public int Number { get; set; }

        public NumberAnalyzer(int number)
        {
            Number = number;
        }

        public bool Palindrom
        {
            get
            {
                string s = Math.Abs(Number).ToString();
                char[] arr = s.ToCharArray();
                Array.Reverse(arr);
                return s == new string(arr);
            }
        }

        public int ReversedNumber
        {
            get
            {
                string s = Math.Abs(Number).ToString();
                char[] arr = s.ToCharArray();
                Array.Reverse(arr);
                int reversed = int.Parse(new string(arr));
                return Number < 0 ? -reversed : reversed;
            }
        }

        public int CountDigit(int digit)
        {
            int count = 0;
            foreach (char c in Math.Abs(Number).ToString())
                if (c == digit.ToString()[0])
                    count++;
            return count;
        }

        public bool Even => Number % 2 == 0;
        public bool Odd => Number % 2 != 0;
        public bool Negative => Number < 0;

        public bool Simple
        {
            get
            {
                if (Number <= 1) return false;
                for (int i = 2; i <= Math.Sqrt(Number); i++)
                    if (Number % i == 0)
                        return false;
                return true;
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // 1
            Console.WriteLine("===== Завдання 1 =====");
            City city = new City();
            city.InputData();
            city.PrintData();

            // 2
            Console.WriteLine("\n===== Завдання 2 =====");
            int[] original = { 1, 2, 6, -1, 88, 7, 6 };
            int[] filter = { 6, 88, 7 };

            int[] result = ArrayFilter.Filter(original, filter);
            Console.WriteLine("Результат фільтрації:");
            Console.WriteLine(string.Join(" ", result));

            // 3
            Console.WriteLine("\n===== Завдання 3 =====");
            Console.Write("Введіть розмір квадрата: ");
            int size = int.Parse(Console.ReadLine());

            Console.Write("Введіть символ: ");
            char symbol = Console.ReadKey().KeyChar;
            Console.WriteLine();

            SquareDrawer square = new SquareDrawer(size, symbol);
            square.Draw();

            // 4
            Console.WriteLine("\n===== Завдання 4 =====");
            Console.Write("Введіть число: ");
            int number = int.Parse(Console.ReadLine());

            NumberAnalyzer num = new NumberAnalyzer(number);

            Console.WriteLine($"Паліндром: {num.Palindrom}");
            Console.WriteLine($"Перевернуте число: {num.ReversedNumber}");
            Console.Write("Яку цифру порахувати?: ");
            int digit = int.Parse(Console.ReadLine());
            Console.WriteLine($"Кількість входжень: {num.CountDigit(digit)}");
            Console.WriteLine($"Парне: {num.Even}");
            Console.WriteLine($"Непарне: {num.Odd}");
            Console.WriteLine($"Від'ємне: {num.Negative}");
            Console.WriteLine($"Просте: {num.Simple}");
        }
    }
}

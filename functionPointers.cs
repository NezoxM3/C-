using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Homework
{
    class Program
    {
        static void Main(string[] args)
        {
            // 1
            Console.Write("Введіть повідомлення: ");
            string message = Console.ReadLine();

            Console.Write("Введіть операції (наприклад 1,3,5): ");
            string[] operations = Console.ReadLine().Split(',');

            Action<string> countChars = s =>
                Console.WriteLine($"Кількість символів: {s.Length}");

            Action<string> countWords = s =>
                Console.WriteLine($"Кількість слів: {s.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length}");

            Action<string> toUpper = s =>
                Console.WriteLine($"Верхній регістр: {s.ToUpper()}");

            Action<string> toLower = s =>
                Console.WriteLine($"Нижній регістр: {s.ToLower()}");

            Action<string> replaceVowels = s =>
            {
                string vowels = "aeiouAEIOUаеєиіїоуюяАЕЄИІЇОУЮЯ";
                StringBuilder sb = new StringBuilder();
                foreach (char c in s)
                    sb.Append(vowels.Contains(c) ? '*' : c);
                Console.WriteLine($"Без голосних: {sb}");
            };

            foreach (var op in operations)
            {
                switch (op.Trim())
                {
                    case "1": countChars(message); break;
                    case "2": countWords(message); break;
                    case "3": toUpper(message); break;
                    case "4": toLower(message); break;
                    case "5": replaceVowels(message); break;
                }
            }

            // 2
            int number = 121;
            Console.WriteLine($"\n{number} паліндром: {number.IsPalindrome()}");
            Console.WriteLine($"{number} ступінь двійки: {number.IsPowerOfTwo()}");
            Console.WriteLine($"Сума цифр: {number.SumDigits()}");
            Console.WriteLine($"Римське число: {number.ToRoman()}");

            // 3
            int primeTest = 17;
            Console.WriteLine($"\n{primeTest} просте: {primeTest.IsPrime()}");

            // 4
            Console.WriteLine($"Голосних у рядку: {message.CountVowels()}");

            // 5
            Console.WriteLine($"Приголосних у рядку: {message.CountConsonants()}");

            // 6
            Console.WriteLine($"Кількість речень: {message.CountSentences()}");

            // 7
            Console.WriteLine($"Перевернутий рядок: {message.ReverseString()}");

            // 8
            string brackets = "[{}]";
            Console.WriteLine($"Валідність дужок '{brackets}': {brackets.AreBracketsValid()}");

            // 9
            Backpack backpack = new Backpack("чорний", 3);

            backpack.ItemAdded += delegate (string item)
            {
                Console.WriteLine($"До рюкзака додати предмет: \"{item}\"");
            };

            backpack.ItemRemoved += delegate (string item)
            {
                Console.WriteLine($"З рюкзака видалили предмет \"{item}\"");
            };

            backpack.ColorChanged += delegate (string oldColor, string newColor)
            {
                Console.WriteLine($"Колір рюкзака змінили з \"{oldColor}\" на \"{newColor}\"");
            };

            try
            {
                backpack.AddItem("Книга");
                backpack.AddItem("Ноутбук");
                backpack.AddItem("Пляшка");
                backpack.RemoveItem("Книга");
                backpack.ChangeColor("білий");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }

    public static class IntExtensions
    {
        public static bool IsPalindrome(this int number)
        {
            string s = number.ToString();
            return s == new string(s.Reverse().ToArray());
        }

        public static bool IsPowerOfTwo(this int number)
        {
            return number > 0 && (number & (number - 1)) == 0;
        }

        public static int SumDigits(this int number)
        {
            return number.ToString().Sum(c => c - '0');
        }

        public static string ToRoman(this int number)
        {
            var map = new Dictionary<int, string>
            {
                {1000,"M"},{900,"CM"},{500,"D"},{400,"CD"},
                {100,"C"},{90,"XC"},{50,"L"},{40,"XL"},
                {10,"X"},{9,"IX"},{5,"V"},{4,"IV"},{1,"I"}
            };

            StringBuilder result = new StringBuilder();
            foreach (var item in map)
            {
                while (number >= item.Key)
                {
                    result.Append(item.Value);
                    number -= item.Key;
                }
            }
            return result.ToString();
        }

        public static bool IsPrime(this int number)
        {
            if (number < 2) return false;
            for (int i = 2; i <= Math.Sqrt(number); i++)
                if (number % i == 0)
                    return false;
            return true;
        }
    }

    public static class StringExtensions
    {
        public static int CountVowels(this string s)
        {
            string vowels = "aeiouAEIOUаеєиіїоуюяАЕЄИІЇОУЮЯ";
            return s.Count(c => vowels.Contains(c));
        }

        public static int CountConsonants(this string s)
        {
            string vowels = "aeiouAEIOUаеєиіїоуюяАЕЄИІЇОУЮЯ";
            return s.Count(c => char.IsLetter(c) && !vowels.Contains(c));
        }

        public static int CountSentences(this string s)
        {
            return s.Count(c => c == '.' || c == '!' || c == '?');
        }

        public static string ReverseString(this string s)
        {
            return new string(s.Reverse().ToArray());
        }

        public static bool AreBracketsValid(this string s)
        {
            Stack<char> stack = new Stack<char>();
            Dictionary<char, char> pairs = new Dictionary<char, char>
            {
                {')','('},{'}','{'},{']','['}
            };

            foreach (char c in s)
            {
                if (pairs.Values.Contains(c))
                    stack.Push(c);
                else if (pairs.ContainsKey(c))
                {
                    if (stack.Count == 0 || stack.Pop() != pairs[c])
                        return false;
                }
            }
            return stack.Count == 0;
        }
    }

    public class Backpack
    {
        public string Color { get; private set; }
        public int Capacity { get; }
        private List<string> items = new List<string>();

        public event Action<string> ItemAdded;
        public event Action<string> ItemRemoved;
        public event Action<string, string> ColorChanged;

        public Backpack(string color, int capacity)
        {
            Color = color;
            Capacity = capacity;
        }

        public void AddItem(string item)
        {
            if (items.Count >= Capacity)
                throw new Exception("Перевищено обсяг рюкзака!");
            items.Add(item);
            ItemAdded?.Invoke(item);
        }

        public void RemoveItem(string item)
        {
            if (items.Remove(item))
                ItemRemoved?.Invoke(item);
        }

        public void ChangeColor(string newColor)
        {
            string oldColor = Color;
            Color = newColor;
            ColorChanged?.Invoke(oldColor, newColor);
        }
    }
}
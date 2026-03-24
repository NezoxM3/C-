using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

class Program
{
    // Завдання 1
    public static T Max<T>(T a, T b, T c) where T : IComparable<T>
    {
        T max = a;
        if (b.CompareTo(max) > 0) max = b;
        if (c.CompareTo(max) > 0) max = c;
        return max;
    }

    // Завдання 2
    public static T Min<T>(T a, T b, T c) where T : IComparable<T>
    {
        T min = a;
        if (b.CompareTo(min) < 0) min = b;
        if (c.CompareTo(min) < 0) min = c;
        return min;
    }

    // Завдання 3
    public static T Sum<T>(T[] array)
    {
        dynamic sum = default(T);
        foreach (var item in array)
        {
            sum += item;
        }
        return sum;
    }

    // Завдання 4
    public class MyStack<T>
    {
        private List<T> items = new List<T>();

        public int Count => items.Count;

        public void Push(T item)
        {
            items.Add(item);
        }

        public T Pop()
        {
            if (items.Count == 0)
                throw new InvalidOperationException("Stack is empty");

            T item = items[^1];
            items.RemoveAt(items.Count - 1);
            return item;
        }

        public T Peek()
        {
            if (items.Count == 0)
                throw new InvalidOperationException("Stack is empty");

            return items[^1];
        }
    }

    // Завдання 5
    public class Alphabet : IEnumerable<char>
    {
        private char[] letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();

        public IEnumerator<char> GetEnumerator()
        {
            foreach (var letter in letters)
                yield return letter;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    // Завдання 6
    public static List<T> FilterByTwoCriteria<T>(
        IEnumerable<T> collection,
        Func<T, bool> predicate1,
        Func<T, bool> predicate2)
    {
        List<T> result = new List<T>();

        foreach (var item in collection)
        {
            if (predicate1(item) && predicate2(item))
                result.Add(item);
        }

        return result;
    }

    // Завдання 7
    public class Apartment : IEnumerable<string>
    {
        public List<string> Residents { get; set; } = new List<string>();

        public IEnumerator<string> GetEnumerator()
        {
            foreach (var person in Residents)
                yield return person;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    public class House : IEnumerable<Apartment>
    {
        public List<Apartment> Apartments { get; set; } = new List<Apartment>();

        public IEnumerator<Apartment> GetEnumerator()
        {
            foreach (var apt in Apartments)
                yield return apt;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    // Завдання 8
    public class Book
    {
        public string Title { get; set; }
        public string Author { get; set; }
        public int Year { get; set; }
        public string Genre { get; set; }
    }

    public class EBook : Book
    {
        public double FileSize { get; set; }
    }

    public class Library<T> : IEnumerable<T> where T : Book
    {
        private List<T> books = new List<T>();

        public void Add(T book)
        {
            books.Add(book);
        }

        public IEnumerable<T> Find(Func<T, bool> predicate)
        {
            foreach (var book in books)
            {
                if (predicate(book))
                    yield return book;
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            foreach (var book in books)
                yield return book;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    static void Main()
    {
        // 1
        Console.WriteLine("Max: " + Max(3, 7, 5));
        // 2
        Console.WriteLine("Min: " + Min(3, 7, 5));
        // 3
        Console.WriteLine("Sum: " + Sum(new int[] { 1, 2, 3, 4 }));

        // 4
        var stack = new MyStack<int>();
        stack.Push(10);
        stack.Push(20);
        Console.WriteLine("Pop: " + stack.Pop());

        // 5
        var alphabet = new Alphabet();
        Console.Write("Alphabet: ");
        foreach (var letter in alphabet)
            Console.Write(letter + " ");
        Console.WriteLine();

        // 6
        var numbers = new List<int> { 1, 2, 3, 4, 5, 6 };
        var filtered = FilterByTwoCriteria(numbers, x => x > 2, x => x % 2 == 0);
        Console.WriteLine("Filtered: " + string.Join(", ", filtered));

        // 7
        var house = new House();
        var apt1 = new Apartment();
        apt1.Residents.AddRange(new[] { "John", "Anna" });
        house.Apartments.Add(apt1);

        Console.WriteLine("Residents:");
        foreach (var apt in house)
        {
            foreach (var person in apt)
                Console.WriteLine(person);
        }

        // 8
        var library = new Library<Book>();
        library.Add(new Book { Title = "Book1", Author = "Author1", Year = 2020, Genre = "Sci-Fi" });
        library.Add(new Book { Title = "Book2", Author = "Author2", Year = 2018, Genre = "Drama" });

        Console.WriteLine("Books:");
        foreach (var book in library)
            Console.WriteLine(book.Title);

        var found = library.Find(b => b.Genre == "Sci-Fi");
        foreach (var book in found)
            Console.WriteLine("Found: " + book.Title);
    }
}
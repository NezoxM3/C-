using System;
using System.Collections.Generic;
using System.Linq;

public class Student
{
    public string FullName { get; set; }
    public int Id { get; set; }
    public string Group { get; set; }

    public override bool Equals(object obj)
    {
        return obj is Student s && s.Id == Id;
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }
}

public class CourseProgress
{
    public string CourseName { get; set; }
    public int Score { get; set; }
    public List<string> CompletedTopics { get; set; } = new();
    public string CurrentTopic { get; set; }
}

public class EducationSystem
{
    private Dictionary<Student, List<CourseProgress>> data = new();

    public void AddOrUpdateStudent(Student student)
    {
        if (!data.ContainsKey(student))
            data[student] = new List<CourseProgress>();
    }

    public void AddCourse(Student student, CourseProgress course)
    {
        AddOrUpdateStudent(student);
        data[student].Add(course);
    }

    public List<Student> FilterStudents(Predicate<Student> predicate)
    {
        return data.Keys.Where(s => predicate(s)).ToList();
    }

    public List<Student> SortStudents(Comparison<Student> comparison)
    {
        var list = data.Keys.ToList();
        list.Sort(comparison);
        return list;
    }

    public Dictionary<Student, List<CourseProgress>> GetAll() => data;
}

class Program
{
    static void Main()
    {
        var dictionary = new Dictionary<string, List<string>>()
        {
            ["hello"] = new List<string> { "привіт", "вітаю" }
        };

        dictionary["hello"][0] = "здрастуй";

        Console.WriteLine("Переклад hello:");
        foreach (var t in dictionary["hello"])
            Console.WriteLine(t);

        var system = new EducationSystem();

        var s1 = new Student { FullName = "Іван Петренко", Id = 1, Group = "A1" };
        var s2 = new Student { FullName = "Марія Іванова", Id = 2, Group = "B1" };

        system.AddCourse(s1, new CourseProgress
        {
            CourseName = "C#",
            Score = 90,
            CurrentTopic = "Generics"
        });

        system.AddCourse(s2, new CourseProgress
        {
            CourseName = "Java",
            Score = 75,
            CurrentTopic = "Streams"
        });

        Predicate<Student> groupFilter = s => s.Group == "A1";
        var filtered = system.FilterStudents(groupFilter);

        Console.WriteLine("\nФільтр по групі A1:");
        filtered.ForEach(s => Console.WriteLine(s.FullName));

        Comparison<Student> compareById = (a, b) => a.Id.CompareTo(b.Id);
        var sorted = system.SortStudents(compareById);

        Console.WriteLine("\nСортування:");
        sorted.ForEach(s => Console.WriteLine(s.FullName));


        Func<int, string> evenCheck = x => x % 2 == 0 ? "парне" : "непарне";
        Console.WriteLine("\n4 -> " + evenCheck(4));

        Func<int, int, double> power = (x, p) => Math.Pow(x, p);
        Console.WriteLine("2^3 = " + power(2, 3));

        Func<double, double, double> power2 = (x, p) => Math.Pow(x, p);
        Console.WriteLine("3^2 = " + power2(3, 2));

        Predicate<DayOfWeek> isWeekend = d => d == DayOfWeek.Saturday || d == DayOfWeek.Sunday;
        Console.WriteLine("Sunday -> " + isWeekend(DayOfWeek.Sunday));

        Func<int[], (int value, int index)> maxWithIndex = arr =>
        {
            int max = arr[0], idx = 0;
            for (int i = 1; i < arr.Length; i++)
                if (arr[i] > max)
                {
                    max = arr[i];
                    idx = i;
                }
            return (max, idx);
        };

        var maxResult = maxWithIndex(new[] { 1, 5, 3, 9, 2 });
        Console.WriteLine($"Max: {maxResult.value}, Index: {maxResult.index}");

        Func<int[], (int value, int index)> minWithIndex = arr =>
        {
            int min = arr[0], idx = 0;
            for (int i = 1; i < arr.Length; i++)
                if (arr[i] < min)
                {
                    min = arr[i];
                    idx = i;
                }
            return (min, idx);
        };

        var minResult = minWithIndex(new[] { 1, 5, 3, -2, 2 });
        Console.WriteLine($"Min: {minResult.value}, Index: {minResult.index}");

        Func<int[], int[]> oddFilter = arr => arr.Where(x => x % 2 != 0).ToArray();
        var odds = oddFilter(new[] { 1, 2, 3, 4, 5 });

        Console.WriteLine("Непарні: " + string.Join(", ", odds));

        Action<string> print = s => Console.WriteLine("Action: " + s);
        print("Hello!");
    }
}
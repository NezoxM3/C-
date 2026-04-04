using System;
using System.Collections.Generic;
using System.Linq;

class Program
{
    static void Main()
    {
        var people = new List<Person>
        {
            new Person
            {
                Id = 1,
                FirstName = "Ivan",
                LastName = "Petrenko",
                Age = 28,
                Skills = new List<Skill>
                {
                    new Skill { Id = 1, Name = "C#", Level = 70 },
                    new Skill { Id = 2, Name = "SQL", Level = 60 },
                    new Skill { Id = 3, Name = "HTML", Level = 40 }
                }
            },
            new Person
            {
                Id = 2,
                FirstName = "Oleg",
                LastName = "Shevchenko",
                Age = 22,
                Skills = new List<Skill>
                {
                    new Skill { Id = 4, Name = "Python", Level = 80 },
                    new Skill { Id = 5, Name = "C#", Level = 55 }
                }
            },
            new Person
            {
                Id = 3,
                FirstName = "Anna",
                LastName = "Koval",
                Age = 31,
                Skills = new List<Skill>
                {
                    new Skill { Id = 6, Name = "C++", Level = 65 }
                }
            }
        };

        // 1
        Console.WriteLine("1:");
        var olderThan25 = people
            .Where(p => p.Age > 25)
            .Select(p => $"{p.FirstName} {p.LastName}");

        foreach (var p in olderThan25)
            Console.WriteLine(p);

        // 2
        Console.WriteLine("\n2:");
        var stats = people.Select(p => new
        {
            FullName = $"{p.LastName} {p.FirstName}",
            SkillCount = p.Skills.Count,
            MaxLevel = p.Skills.Any() ? p.Skills.Max(s => s.Level) : 0
        });

        foreach (var s in stats)
            Console.WriteLine($"{s.FullName} — {s.SkillCount} — {s.MaxLevel}");

        // 3
        Console.WriteLine("\n3:");
        var csharp50 = people
            .Where(p => p.Skills.Any(s => s.Name == "C#" && s.Level >= 50));

        foreach (var p in csharp50)
            Console.WriteLine($"{p.FirstName} {p.LastName}");

        // 4
        Console.WriteLine("\n4:");
        var noSql = people
            .Where(p => !p.Skills.Any(s => s.Name == "SQL"));

        foreach (var p in noSql)
            Console.WriteLine($"{p.FirstName} {p.LastName}");

        // 5
        Console.WriteLine("\n5:");
        var skillStats = people
            .SelectMany(p => p.Skills.Select(s => new { p.Id, s.Name }))
            .GroupBy(x => x.Name)
            .Select(g => new
            {
                Skill = g.Key,
                Count = g.Select(x => x.Id).Distinct().Count()
            });

        foreach (var s in skillStats)
            Console.WriteLine($"{s.Skill}: {s.Count}");

        // 6
        Console.WriteLine("\n6:");
        int countUnder30 = people.Count(p => p.Age < 30);
        Console.WriteLine(countUnder30);

        // 7
        Console.WriteLine("\n7:");
        var skilled = people
            .Where(p => p.Skills.Count > 2);

        foreach (var p in skilled)
            Console.WriteLine($"{p.FirstName} {p.LastName}");

        // 8
        Console.WriteLine("\n8:");
        var csharpSkills = people
            .SelectMany(p => p.Skills)
            .Where(s => s.Name == "C#");

        double avg = csharpSkills.Any()
            ? csharpSkills.Average(s => s.Level)
            : 0;

        Console.WriteLine(avg);

        // 9
        Console.WriteLine("\n9:");
        var result = people
            .Where(p => p.Skills.Any(s => s.Name == "C#"))
            .Select(p => $"{p.FirstName} {p.LastName} - {DateTime.Now.Year - p.Age}");

        foreach (var r in result)
            Console.WriteLine(r);

        // 10
        Console.WriteLine("\n10:");
        int maxSkills = people.Max(p => p.Skills.Count);

        var topPeople = people
            .Where(p => p.Skills.Count == maxSkills);

        foreach (var p in topPeople)
            Console.WriteLine($"{p.FirstName} {p.LastName}");

        // 11
        Console.WriteLine("\n11:");
        var fullStats = people
            .SelectMany(p => p.Skills.Select(s => new { p.Id, s.Name, s.Level }))
            .GroupBy(x => x.Name)
            .Select(g => new
            {
                Skill = g.Key,
                PeopleCount = g.Select(x => x.Id).Distinct().Count(),
                MaxLevel = g.Max(x => x.Level)
            });

        foreach (var s in fullStats)
            Console.WriteLine($"{s.Skill} — {s.PeopleCount} — {s.MaxLevel}");
    }
}

class Person
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public int Age { get; set; }
    public List<Skill> Skills { get; set; }
}

class Skill
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int Level { get; set; }
}
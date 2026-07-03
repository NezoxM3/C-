using Microsoft.EntityFrameworkCore;
using System.Linq;

class Program
{
    static void Main()
    {
        using var db = new AppDbContext();

        db.Database.EnsureCreated();

        if (!db.Students.Any())
        {
            var programming = new Course
            {
                Name = "Programming",
                Teacher = "John Smith",
                Credits = 5
            };

            var databases = new Course
            {
                Name = "Databases",
                Teacher = "Alice Brown",
                Credits = 4
            };

            db.Courses.AddRange(programming, databases);
            db.SaveChanges();

            var student1 = new Student
            {
                FirstName = "Max",
                LastName = "Johnson",
                Age = 20
            };

            var student2 = new Student
            {
                FirstName = "Anna",
                LastName = "Wilson",
                Age = 21
            };

            db.Students.AddRange(student1, student2);
            db.SaveChanges();

            db.Grades.AddRange(
                new Grade
                {
                    StudentId = student1.Id,
                    CourseId = programming.Id,
                    Value = 95
                },
                new Grade
                {
                    StudentId = student1.Id,
                    CourseId = databases.Id,
                    Value = 88
                },
                new Grade
                {
                    StudentId = student2.Id,
                    CourseId = programming.Id,
                    Value = 91
                });

            db.SaveChanges();
        }

        var students = db.Students
            .Include(s => s.Grades)
            .ThenInclude(g => g.Course)
            .ToList();

        foreach (var student in students)
        {
            Console.WriteLine($"{student.FirstName} {student.LastName}");

            foreach (var grade in student.Grades)
            {
                Console.WriteLine($"   {grade.Course.Name}: {grade.Value}");
            }

            Console.WriteLine();
        }
    }
}
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
            // Create group
            var group = new Group
            {
                Name = "CS-101"
            };

            db.Groups.Add(group);

            // Create courses
            var course1 = new Course
            {
                Name = "Programming",
                Teacher = "John Smith",
                Credits = 5
            };

            var course2 = new Course
            {
                Name = "Databases",
                Teacher = "Alice Brown",
                Credits = 4
            };

            db.Courses.AddRange(course1, course2);
            db.SaveChanges();

            // Create students
            var student1 = new Student
            {
                FirstName = "Max",
                LastName = "Johnson",
                Age = 20,
                GroupId = group.Id
            };

            var student2 = new Student
            {
                FirstName = "Anna",
                LastName = "Wilson",
                Age = 21,
                GroupId = group.Id
            };

            db.Students.AddRange(student1, student2);
            db.SaveChanges();

            // Create grades
            db.Grades.AddRange(
                new Grade
                {
                    StudentId = student1.Id,
                    CourseId = course1.Id,
                    Value = 95
                },
                new Grade
                {
                    StudentId = student1.Id,
                    CourseId = course2.Id,
                    Value = 88
                },
                new Grade
                {
                    StudentId = student2.Id,
                    CourseId = course1.Id,
                    Value = 91
                }
            );

            db.SaveChanges();
        }

        var students = db.Students
            .Include(s => s.Group)
            .Include(s => s.Grades)
            .ThenInclude(g => g.Course)
            .ToList();

        Console.WriteLine("=== Students ===");
        Console.WriteLine();

        foreach (var student in students)
        {
            Console.WriteLine($"{student.FirstName} {student.LastName}");
            Console.WriteLine($"Group: {student.Group?.Name}");

            foreach (var grade in student.Grades)
            {
                Console.WriteLine($"   {grade.Course.Name}: {grade.Value}");
            }

            Console.WriteLine();
        }
    }
}
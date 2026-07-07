using System.Collections.Generic;

public class Student
{
    public int Id { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public int Age { get; set; }

    public int? GroupId { get; set; }

    public Group? Group { get; set; }

    public List<Grade> Grades { get; set; } = new();
}
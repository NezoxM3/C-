using System.Collections.Generic;

public class Manufacturer
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Country { get; set; } = string.Empty;

    public List<Car> Cars { get; set; } = new();
}
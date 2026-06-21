using System.Collections.Generic;

public class Car
{
    public int Id { get; set; }

    public string Model { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public int ManufacturerId { get; set; }

    public Manufacturer Manufacturer { get; set; } = null!;

    public List<Review> Reviews { get; set; } = new();
}
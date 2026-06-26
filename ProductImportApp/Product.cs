public class Product
{
    public int Id { get; set; }

    public string ProductCode { get; set; } = string.Empty;

    public int BrandId { get; set; }

    public Brand Brand { get; set; } = null!;

    public string Name { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public bool InStock { get; set; }

    public int Quantity { get; set; }
}
using System.Globalization;
using Microsoft.EntityFrameworkCore;

class Program
{
    static void Main()
    {
        using var db = new AppDbContext();

        db.Database.EnsureCreated();

        string csvPath = "products.csv";

        if (!File.Exists(csvPath))
        {
            Console.WriteLine("products.csv not found.");
            return;
        }

        var lines = File.ReadAllLines(csvPath);

        Console.WriteLine($"Rows found: {lines.Length - 1}");

        foreach (var line in lines.Skip(1))
        {
            var columns = line.Split(',');

            if (columns.Length < 15)
                continue;

            string productCode = columns[0].Trim();
            string productName = columns[1].Trim();
            string brandName = columns[5].Trim();

            decimal price = ParseDecimal(columns[10]);

            int qtyUt = ParseInt(columns[13]);
            int qtyKy = ParseInt(columns[14]);

            int quantity = qtyUt + qtyKy;

            bool inStock = quantity > 0;

            var brand = db.Brands
                .FirstOrDefault(b => b.Name == brandName);

            if (brand == null)
            {
                brand = new Brand
                {
                    Name = brandName
                };

                db.Brands.Add(brand);
                db.SaveChanges();
            }

            var product = db.Products
                .FirstOrDefault(
                    p => p.ProductCode == productCode);

            if (product == null)
            {
                product = new Product
                {
                    ProductCode = productCode,
                    Name = productName,
                    Price = price,
                    Quantity = quantity,
                    InStock = inStock,
                    BrandId = brand.Id
                };

                db.Products.Add(product);
            }
            else
            {
                product.Name = productName;
                product.Price = price;
                product.Quantity = quantity;
                product.InStock = inStock;
                product.BrandId = brand.Id;
            }
        }

        db.SaveChanges();

        Console.WriteLine();
        Console.WriteLine("Import completed.");

        Console.WriteLine();
        Console.WriteLine("Products in database:");

        var products = db.Products
            .Include(p => p.Brand)
            .Take(20)
            .ToList();

        foreach (var product in products)
        {
            Console.WriteLine(
                $"{product.ProductCode} | " +
                $"{product.Brand.Name} | " +
                $"{product.Name} | " +
                $"{product.Price} | " +
                $"{product.Quantity}");
        }
    }

    static decimal ParseDecimal(string value)
    {
        value = value.Replace("\"", "");

        decimal.TryParse(
            value,
            NumberStyles.Any,
            CultureInfo.InvariantCulture,
            out decimal result);

        return result;
    }

    static int ParseInt(string value)
    {
        int.TryParse(value, out int result);

        return result;
    }
}
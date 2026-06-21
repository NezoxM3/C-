using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

class Program
{
    static void Main()
    {
        using var db = new AppDbContext();

        db.Database.EnsureCreated();

        SeedData(db);

        Console.WriteLine("=== ALL CARS ===");
        ShowCars(db);

        Console.WriteLine("\n=== ADD REVIEW ===");
        AddReview(db, 1, "Excellent car", 5);

        Console.WriteLine("\n=== REVIEWS FOR CAR 1 ===");
        ShowReviewsForCar(db, 1);

        Console.WriteLine("\n=== SEARCH REVIEWS ===");
        SearchReviews(db, "Toyota", "Camry");

        Console.WriteLine("\n=== SEARCH CARS BY PRICE ===");
        SearchCarsByPrice(db, 15000, 40000);

        Console.WriteLine("\n=== SEARCH CARS BY MANUFACTURER ===");
        SearchCarsByManufacturer(db, "BMW");

        Console.WriteLine("\n=== MANUFACTURERS ===");
        ShowManufacturers(db);

        Console.WriteLine("\n=== ADD MANUFACTURER ===");
        AddManufacturer(db, "Audi", "Germany");

        Console.WriteLine("\n=== UPDATE MANUFACTURER ===");
        UpdateManufacturer(db, 1, "Toyota Updated");

        Console.WriteLine("\n=== DELETE MANUFACTURER ===");
        DeleteManufacturer(db, 3);

        Console.WriteLine("\n=== FINAL MANUFACTURERS ===");
        ShowManufacturers(db);
    }

    static void SeedData(AppDbContext db)
    {
        if (db.Manufacturers.Any())
            return;

        var toyota = new Manufacturer
        {
            Name = "Toyota",
            Country = "Japan"
        };

        var bmw = new Manufacturer
        {
            Name = "BMW",
            Country = "Germany"
        };

        db.Manufacturers.AddRange(toyota, bmw);
        db.SaveChanges();

        db.Cars.AddRange(
            new Car
            {
                Model = "Camry",
                Price = 30000,
                ManufacturerId = toyota.Id
            },
            new Car
            {
                Model = "Corolla",
                Price = 22000,
                ManufacturerId = toyota.Id
            },
            new Car
            {
                Model = "X5",
                Price = 55000,
                ManufacturerId = bmw.Id
            }
        );

        db.SaveChanges();
    }

    static void ShowCars(AppDbContext db)
    {
        var cars = db.Cars
            .Include(c => c.Manufacturer)
            .ToList();

        foreach (var car in cars)
        {
            Console.WriteLine(
                $"{car.Id} | {car.Manufacturer.Name} | {car.Model} | {car.Price}$");
        }
    }

    static void AddReview(
        AppDbContext db,
        int carId,
        string text,
        int rating)
    {
        var review = new Review
        {
            CarId = carId,
            Text = text,
            Rating = rating
        };

        db.Reviews.Add(review);
        db.SaveChanges();

        Console.WriteLine("Review added.");
    }

    static void ShowReviewsForCar(
        AppDbContext db,
        int carId)
    {
        var reviews = db.Reviews
            .Where(r => r.CarId == carId)
            .ToList();

        foreach (var review in reviews)
        {
            Console.WriteLine(
                $"Rating: {review.Rating} | {review.Text}");
        }
    }

    static void SearchReviews(
        AppDbContext db,
        string manufacturer,
        string model)
    {
        var reviews = db.Reviews
            .Include(r => r.Car)
            .ThenInclude(c => c.Manufacturer)
            .Where(r =>
                r.Car.Model == model &&
                r.Car.Manufacturer.Name == manufacturer)
            .ToList();

        foreach (var review in reviews)
        {
            Console.WriteLine(
                $"{manufacturer} {model}: {review.Text}");
        }
    }

    static void SearchCarsByPrice(
        AppDbContext db,
        decimal min,
        decimal max)
    {
        var cars = db.Cars
            .Include(c => c.Manufacturer)
            .Where(c => c.Price >= min &&
                        c.Price <= max)
            .ToList();

        foreach (var car in cars)
        {
            Console.WriteLine(
                $"{car.Manufacturer.Name} {car.Model} - {car.Price}$");
        }
    }

    static void SearchCarsByManufacturer(
        AppDbContext db,
        string manufacturer)
    {
        var cars = db.Cars
            .Include(c => c.Manufacturer)
            .Where(c => c.Manufacturer.Name == manufacturer)
            .ToList();

        foreach (var car in cars)
        {
            Console.WriteLine(
                $"{car.Manufacturer.Name} {car.Model}");
        }
    }

    static void ShowManufacturers(AppDbContext db)
    {
        foreach (var manufacturer in db.Manufacturers)
        {
            Console.WriteLine(
                $"{manufacturer.Id} | {manufacturer.Name} | {manufacturer.Country}");
        }
    }

    static void AddManufacturer(
        AppDbContext db,
        string name,
        string country)
    {
        db.Manufacturers.Add(
            new Manufacturer
            {
                Name = name,
                Country = country
            });

        db.SaveChanges();

        Console.WriteLine("Manufacturer added.");
    }

    static void UpdateManufacturer(
        AppDbContext db,
        int id,
        string newName)
    {
        var manufacturer =
            db.Manufacturers.Find(id);

        if (manufacturer == null)
            return;

        manufacturer.Name = newName;

        db.SaveChanges();

        Console.WriteLine("Manufacturer updated.");
    }

    static void DeleteManufacturer(
        AppDbContext db,
        int id)
    {
        var manufacturer =
            db.Manufacturers.Find(id);

        if (manufacturer == null)
            return;

        db.Manufacturers.Remove(manufacturer);

        db.SaveChanges();

        Console.WriteLine("Manufacturer deleted.");
    }
}
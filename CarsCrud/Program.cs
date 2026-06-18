using System;
using System.Linq;

class Program
{
    static void Main()
    {
        using var db = new AppDbContext();

        db.Database.EnsureCreated();

        Console.WriteLine("=== CREATE ===");

        CreateCar(
            db,
            "Toyota",
            "Camry",
            2023);

        CreateCar(
            db,
            "BMW",
            "X5",
            2022);

        Console.WriteLine();
        Console.WriteLine("=== READ ===");

        ReadCars(db);

        Console.WriteLine();
        Console.WriteLine("=== UPDATE ===");

        UpdateCar(
            db,
            1,
            "Toyota",
            "Corolla",
            2024);

        ReadCars(db);

        Console.WriteLine();
        Console.WriteLine("=== DELETE ===");

        DeleteCar(db, 2);

        ReadCars(db);

        Console.WriteLine();
        Console.WriteLine("Done!");
    }

    static void CreateCar(
        AppDbContext db,
        string brand,
        string model,
        int year)
    {
        var car = new Car
        {
            Brand = brand,
            Model = model,
            Year = year
        };

        db.Cars.Add(car);

        db.SaveChanges();

        Console.WriteLine(
            $"Added: {brand} {model}");
    }

    static void ReadCars(AppDbContext db)
    {
        var cars = db.Cars.ToList();

        if (!cars.Any())
        {
            Console.WriteLine("No cars found.");
            return;
        }

        foreach (var car in cars)
        {
            Console.WriteLine(
                $"Id: {car.Id} | " +
                $"Brand: {car.Brand} | " +
                $"Model: {car.Model} | " +
                $"Year: {car.Year}");
        }
    }

    static void UpdateCar(
        AppDbContext db,
        int id,
        string brand,
        string model,
        int year)
    {
        var car = db.Cars.Find(id);

        if (car == null)
        {
            Console.WriteLine(
                $"Car with Id {id} not found.");
            return;
        }

        car.Brand = brand;
        car.Model = model;
        car.Year = year;

        db.SaveChanges();

        Console.WriteLine(
            $"Car with Id {id} updated.");
    }

    static void DeleteCar(
        AppDbContext db,
        int id)
    {
        var car = db.Cars.Find(id);

        if (car == null)
        {
            Console.WriteLine(
                $"Car with Id {id} not found.");
            return;
        }

        db.Cars.Remove(car);

        db.SaveChanges();

        Console.WriteLine(
            $"Car with Id {id} deleted.");
    }
}
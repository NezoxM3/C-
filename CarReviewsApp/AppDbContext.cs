using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public DbSet<Manufacturer> Manufacturers { get; set; }

    public DbSet<Car> Cars { get; set; }

    public DbSet<Review> Reviews { get; set; }

    protected override void OnConfiguring(
        DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite(
            "Data Source=cars.db");
    }
}
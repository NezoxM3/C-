using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public DbSet<Car> Cars { get; set; }

    protected override void OnConfiguring(
        DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite(
            "Data Source=cars.db");
    }
}
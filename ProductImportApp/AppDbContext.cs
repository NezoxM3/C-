using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public DbSet<Brand> Brands { get; set; }

    public DbSet<Product> Products { get; set; }

    protected override void OnConfiguring(
        DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite(
            "Data Source=products.db");
    }
}
using Microsoft.EntityFrameworkCore;
using GenAiLab.Web.Models;

namespace GenAiLab.Web.Services;

public class ProductDbContext : DbContext
{
    public ProductDbContext(DbContextOptions<ProductDbContext> options) : base(options)
    {
    }

    public DbSet<ProductInfo> Products { get; set; }
    public DbSet<ProductCategory> Categories { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure ProductInfo entity
        modelBuilder.Entity<ProductInfo>()
            .HasKey(p => p.Id);

        modelBuilder.Entity<ProductInfo>()
            .Property(p => p.Name)
            .IsRequired();

        modelBuilder.Entity<ProductInfo>()
            .Property(p => p.FileName)
            .IsRequired();

        // Configure ProductCategory entity
        modelBuilder.Entity<ProductCategory>()
            .HasKey(c => c.Id);

        modelBuilder.Entity<ProductCategory>()
            .Property(c => c.Name)
            .IsRequired();

        modelBuilder.Entity<ProductCategory>()
            .HasIndex(c => c.Name)
            .IsUnique();
    }    // Helper method to initialize the database
    public static void Initialize(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        using var context = scope.ServiceProvider.GetRequiredService<ProductDbContext>();
        context.Database.EnsureCreated();
    }
}

// New entity for storing categories
public class ProductCategory
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

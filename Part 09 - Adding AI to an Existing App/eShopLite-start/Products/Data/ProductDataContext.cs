using DataEntities;
using Microsoft.EntityFrameworkCore;

namespace Products.Data;

public class ProductDataContext(DbContextOptions<ProductDataContext> options)
    : DbContext(options)
{
    public DbSet<Product> Product => Set<Product>();
}

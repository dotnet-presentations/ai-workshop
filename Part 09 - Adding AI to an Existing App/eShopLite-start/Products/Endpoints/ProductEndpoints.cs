using DataEntities;
using Microsoft.EntityFrameworkCore;
using Products.Data;

namespace Products.Endpoints;

public static class ProductEndpoints
{
    public static void MapProductEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/product");

        group.MapGet("/", async (ProductDataContext db) =>
            await db.Product.ToListAsync())
            .WithName("GetAllProducts");

        group.MapGet("/{id:int}", async (int id, ProductDataContext db) =>
            await db.Product.FindAsync(id) is Product product
                ? Results.Ok(product)
                : Results.NotFound())
            .WithName("GetProductById");

        // Keyword search. This is the method the AI features will sit alongside.
        group.MapGet("/search/{search}", async (string search, ProductDataContext db) =>
            await db.Product
                .Where(p => EF.Functions.Like(p.Name, $"%{search}%")
                         || EF.Functions.Like(p.Description, $"%{search}%"))
                .ToListAsync())
            .WithName("SearchProducts");
    }
}

using DataEntities;
using Microsoft.EntityFrameworkCore;
using Products.Ai;
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

        // Semantic search. The vector store returns ids, the database returns the products.
        group.MapGet("/aisearch/{search}", async (
            string search,
            ProductSemanticSearch semanticSearch,
            ProductDataContext db) =>
        {
            var ids = await semanticSearch.SearchAsync(search);

            if (ids.Count == 0)
            {
                return Results.Ok(new List<Product>());
            }

            var products = await db.Product.Where(p => ids.Contains(p.Id)).ToListAsync();

            // Preserve the ranking the vector search gave us.
            // Use FirstOrDefault + OfType to skip ids that are no longer in the
            // database (e.g., stale entries in the vector index).
            var ordered = ids
                .Select(id => products.FirstOrDefault(p => p.Id == id))
                .OfType<Product>()
                .ToList();

            return Results.Ok(ordered);
        })
            .WithName("AiSearchProducts");
    }
}

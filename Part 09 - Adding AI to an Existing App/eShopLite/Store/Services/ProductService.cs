using System.Net.Http.Json;
using DataEntities;

namespace Store.Services;

public class ProductService(HttpClient httpClient, ILogger<ProductService> logger)
{
    public async Task<List<Product>> GetProducts()
    {
        try
        {
            return await httpClient.GetFromJsonAsync<List<Product>>("/api/product") ?? [];
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to load the product catalog.");
            return [];
        }
    }

    public async Task<List<Product>> SearchProducts(string searchTerm)
    {
        try
        {
            var url = $"/api/product/search/{Uri.EscapeDataString(searchTerm)}";
            return await httpClient.GetFromJsonAsync<List<Product>>(url) ?? [];
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Search failed for term {SearchTerm}.", searchTerm);
            return [];
        }
    }
}

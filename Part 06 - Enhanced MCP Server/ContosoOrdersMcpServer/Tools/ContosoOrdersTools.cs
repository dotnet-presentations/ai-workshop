using System.ComponentModel;
using ModelContextProtocol.Server;

namespace ContosoOrdersMcpServer.Tools;

/// <summary>
/// ContosoOrders business tools for demonstration purposes.
/// These tools can be invoked by MCP clients to interact with the Contoso business system.
/// </summary>
internal class ContosoOrdersTools
{
    private static readonly IReadOnlyDictionary<string, OrderDetails> Orders =
        new Dictionary<string, OrderDetails>
        {
            ["12345"] = new("John Doe", "$150.00", "Shipped",
                ["Camping Tent", "Sleeping Bag"],
                "123 Adventure Lane, Outdoor City, OC 12345", "2025-07-25", "1Z999AA1012345675"),
            ["12346"] = new("Jane Smith", "$89.99", "Processing",
                ["Hiking Boots"],
                "456 Trail Rd, Mountain View, MV 67890", "2025-07-30", null),
            ["12347"] = new("Bob Johnson", "$245.50", "Delivered",
                ["Backpack", "Water Bottle", "Trail Mix"],
                "789 Summit St, Peak Town, PT 11111", "2025-07-20", "1Z999AA1012345676"),
            ["12350"] = new("John Doe", "$75.99", "Delivered",
                ["Sleeping Bag"],
                "123 Adventure Lane, Outdoor City, OC 12345", "2025-07-15", "1Z999AA1012345677")
        };

    [McpServerTool(UseStructuredContent = true)]
    [Description("Retrieves order information from the Contoso business system.")]
    public OrderLookupResult GetOrderDetails(
        [Description("The order ID to look up")] string orderId)
    {
        if (Orders.TryGetValue(orderId, out var order))
        {
            return new OrderLookupResult(true, orderId, order, null);
        }

        return new OrderLookupResult(false, orderId, null, $"Order {orderId} not found in the system.");
    }

    [McpServerTool(UseStructuredContent = true)]
    [Description("Searches for orders by customer name.")]
    public CustomerOrderSearchResult SearchOrdersByCustomer(
        [Description("Customer name to search for")] string customerName)
    {
        var customer = Orders.Values
            .Select(order => order.Customer)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault(name =>
            name.Contains(customerName, StringComparison.OrdinalIgnoreCase));

        if (customer != null)
        {
            var orders = Orders
                .Where(entry => entry.Value.Customer.Equals(customer, StringComparison.OrdinalIgnoreCase))
                .Select(entry => new CustomerOrder(
                    entry.Key,
                    entry.Value.Total,
                    entry.Value.Status,
                    entry.Value.OrderDate))
                .ToArray();

            return new CustomerOrderSearchResult(true, customer, orders, null);
        }

        return new CustomerOrderSearchResult(
            false, customerName, [], $"No orders found for customer '{customerName}'.");
    }

    [McpServerTool(UseStructuredContent = true)]
    [Description("Searches order history for orders containing a product.")]
    public ProductOrderSearchResult SearchOrdersByProduct(
        [Description("Product name to search for in order items")] string productName)
    {
        var orders = Orders
            .Select(entry => new
            {
                entry.Key,
                Order = entry.Value,
                MatchingItems = entry.Value.Items
                    .Where(item => ProductNamesMatch(item, productName))
                    .ToArray()
            })
            .Where(result => result.MatchingItems.Length > 0)
            .OrderByDescending(result => result.Order.OrderDate)
            .Select(result => new ProductOrder(
                result.Key,
                result.Order.Customer,
                result.Order.Total,
                result.Order.Status,
                result.Order.OrderDate,
                result.MatchingItems))
            .ToArray();

        if (orders.Length > 0)
        {
            return new ProductOrderSearchResult(true, productName, orders, null);
        }

        return new ProductOrderSearchResult(
            false, productName, [], $"No orders found containing product '{productName}'.");
    }

    [McpServerTool(UseStructuredContent = true)]
    [Description("Gets inventory status for a specific product.")]
    public ProductInventoryResult GetProductInventory(
        [Description("Product name or SKU to check inventory for")] string productName)
    {
        // Simulate inventory lookup
        var inventory = new Dictionary<string, InventoryDetails>
        {
            ["Camping Tent"] = new("CT-001", 15, "$89.99", "Shelter"),
            ["Sleeping Bag"] = new("SB-002", 23, "$59.99", "Sleep"),
            ["Hiking Boots"] = new("HB-003", 8, "$89.99", "Footwear"),
            ["Backpack"] = new("BP-004", 12, "$129.99", "Gear"),
            ["Water Bottle"] = new("WB-005", 45, "$19.99", "Hydration"),
            ["Trail Mix"] = new("TM-006", 67, "$8.99", "Food")
        };

        var normalizedProductName = productName.Trim();
        var searchKey = inventory.Keys.FirstOrDefault(name =>
            ProductNamesMatch(name, normalizedProductName) ||
            inventory[name].Sku.Equals(normalizedProductName, StringComparison.OrdinalIgnoreCase));

        if (searchKey != null)
        {
            return new ProductInventoryResult(true, searchKey, inventory[searchKey], null);
        }

        return new ProductInventoryResult(
            false, productName, null, $"Product '{productName}' not found in inventory.");
    }

    private static bool ProductNamesMatch(string productName, string searchTerm)
    {
        var normalizedSearchTerm = searchTerm.Trim();
        return normalizedSearchTerm.Length > 0 &&
            (productName.Contains(normalizedSearchTerm, StringComparison.OrdinalIgnoreCase) ||
             normalizedSearchTerm.Contains(productName, StringComparison.OrdinalIgnoreCase));
    }
}

internal sealed record OrderLookupResult(bool Found, string OrderId, OrderDetails? Order, string? Message);

internal sealed record OrderDetails(
    string Customer,
    string Total,
    string Status,
    string[] Items,
    string ShippingAddress,
    string OrderDate,
    string? TrackingNumber);

internal sealed record CustomerOrderSearchResult(
    bool Found,
    string Customer,
    CustomerOrder[] Orders,
    string? Message);

internal sealed record CustomerOrder(string OrderId, string Total, string Status, string Date);

internal sealed record ProductOrderSearchResult(
    bool Found,
    string Product,
    ProductOrder[] Orders,
    string? Message);

internal sealed record ProductOrder(
    string OrderId,
    string Customer,
    string Total,
    string Status,
    string Date,
    string[] MatchingItems);

internal sealed record ProductInventoryResult(
    bool Found,
    string Product,
    InventoryDetails? Details,
    string? Message);

internal sealed record InventoryDetails(string Sku, int InStock, string Price, string Category);

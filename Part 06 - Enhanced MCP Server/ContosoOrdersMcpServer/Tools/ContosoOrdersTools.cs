using System.ComponentModel;
using ModelContextProtocol.Server;

namespace ContosoOrdersMcpServer.Tools;

/// <summary>
/// ContosoOrders business tools for demonstration purposes.
/// These tools can be invoked by MCP clients to interact with the Contoso business system.
/// </summary>
internal class ContosoOrdersTools
{
    [McpServerTool(UseStructuredContent = true)]
    [Description("Retrieves order information from the Contoso business system.")]
    public OrderLookupResult GetOrderDetails(
        [Description("The order ID to look up")] string orderId)
    {
        // Simulate business data lookup
        var orders = new Dictionary<string, OrderDetails>
        {
            ["12345"] = new("John Doe", "$150.00", "Shipped",
                ["Camping Tent", "Sleeping Bag"],
                "123 Adventure Lane, Outdoor City, OC 12345", "2025-07-25", "1Z999AA1012345675"),
            ["12346"] = new("Jane Smith", "$89.99", "Processing",
                ["Hiking Boots"],
                "456 Trail Rd, Mountain View, MV 67890", "2025-07-30", null),
            ["12347"] = new("Bob Johnson", "$245.50", "Delivered",
                ["Backpack", "Water Bottle", "Trail Mix"],
                "789 Summit St, Peak Town, PT 11111", "2025-07-20", "1Z999AA1012345676")
        };

        if (orders.TryGetValue(orderId, out var order))
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
        // Simulate customer search
        var customerOrders = new Dictionary<string, CustomerOrder[]>
        {
            ["John Doe"] = [
                new("12345", "$150.00", "Shipped", "2025-07-25"),
                new("12350", "$75.99", "Delivered", "2025-07-15")
            ],
            ["Jane Smith"] = [new("12346", "$89.99", "Processing", "2025-07-30")],
            ["Bob Johnson"] = [new("12347", "$245.50", "Delivered", "2025-07-20")]
        };

        var searchKey = customerOrders.Keys.FirstOrDefault(name =>
            name.Contains(customerName, StringComparison.OrdinalIgnoreCase));

        if (searchKey != null)
        {
            return new CustomerOrderSearchResult(true, searchKey, customerOrders[searchKey], null);
        }

        return new CustomerOrderSearchResult(
            false, customerName, [], $"No orders found for customer '{customerName}'.");
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

        var searchKey = inventory.Keys.FirstOrDefault(name =>
            name.Contains(productName, StringComparison.OrdinalIgnoreCase));

        if (searchKey != null)
        {
            return new ProductInventoryResult(true, searchKey, inventory[searchKey], null);
        }

        return new ProductInventoryResult(
            false, productName, null, $"Product '{productName}' not found in inventory.");
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

internal sealed record ProductInventoryResult(
    bool Found,
    string Product,
    InventoryDetails? Details,
    string? Message);

internal sealed record InventoryDetails(string Sku, int InStock, string Price, string Category);

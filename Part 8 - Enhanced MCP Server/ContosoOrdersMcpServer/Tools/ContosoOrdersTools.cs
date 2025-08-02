using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol.Server;

/// <summary>
/// ContosoOrders business tools for demonstration purposes.
/// These tools can be invoked by MCP clients to interact with the Contoso business system.
/// </summary>
internal class ContosoOrdersTools
{
    [McpServerTool]
    [Description("Retrieves order information from the Contoso business system.")]
    public async Task<string> GetOrderDetails(
        [Description("The order ID to look up")] string orderId)
    {
        // Simulate business data lookup
        var orders = new Dictionary<string, object>
        {
            ["12345"] = new
            {
                Customer = "John Doe",
                Total = "$150.00",
                Status = "Shipped",
                Items = new[] { "Camping Tent", "Sleeping Bag" },
                ShippingAddress = "123 Adventure Lane, Outdoor City, OC 12345",
                OrderDate = "2025-07-25",
                TrackingNumber = "1Z999AA1012345675"
            },
            ["12346"] = new
            {
                Customer = "Jane Smith",
                Total = "$89.99",
                Status = "Processing",
                Items = new[] { "Hiking Boots" },
                ShippingAddress = "456 Trail Rd, Mountain View, MV 67890",
                OrderDate = "2025-07-30",
                TrackingNumber = null
            },
            ["12347"] = new
            {
                Customer = "Bob Johnson",
                Total = "$245.50",
                Status = "Delivered",
                Items = new[] { "Backpack", "Water Bottle", "Trail Mix" },
                ShippingAddress = "789 Summit St, Peak Town, PT 11111",
                OrderDate = "2025-07-20",
                TrackingNumber = "1Z999AA1012345676"
            }
        };

        if (orders.TryGetValue(orderId, out var order))
        {
            return JsonSerializer.Serialize(order, new JsonSerializerOptions { WriteIndented = true });
        }

        return $"Order {orderId} not found in the system.";
    }

    [McpServerTool]
    [Description("Searches for orders by customer name.")]
    public async Task<string> SearchOrdersByCustomer(
        [Description("Customer name to search for")] string customerName)
    {
        // Simulate customer search
        var customerOrders = new Dictionary<string, object[]>
        {
            ["John Doe"] = new object[] {
                new { OrderId = "12345", Total = "$150.00", Status = "Shipped", Date = "2025-07-25" },
                new { OrderId = "12350", Total = "$75.99", Status = "Delivered", Date = "2025-07-15" }
            },
            ["Jane Smith"] = new object[] {
                new { OrderId = "12346", Total = "$89.99", Status = "Processing", Date = "2025-07-30" }
            },
            ["Bob Johnson"] = new object[] {
                new { OrderId = "12347", Total = "$245.50", Status = "Delivered", Date = "2025-07-20" }
            }
        };

        var searchKey = customerOrders.Keys.FirstOrDefault(name =>
            name.Contains(customerName, StringComparison.OrdinalIgnoreCase));

        if (searchKey != null)
        {
            var result = new { Customer = searchKey, Orders = customerOrders[searchKey] };
            return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        }

        return $"No orders found for customer '{customerName}'.";
    }

    [McpServerTool]
    [Description("Gets inventory status for a specific product.")]
    public async Task<string> GetProductInventory(
        [Description("Product name or SKU to check inventory for")] string productName)
    {
        // Simulate inventory lookup
        var inventory = new Dictionary<string, object>
        {
            ["Camping Tent"] = new { SKU = "CT-001", InStock = 15, Price = "$89.99", Category = "Shelter" },
            ["Sleeping Bag"] = new { SKU = "SB-002", InStock = 23, Price = "$59.99", Category = "Sleep" },
            ["Hiking Boots"] = new { SKU = "HB-003", InStock = 8, Price = "$89.99", Category = "Footwear" },
            ["Backpack"] = new { SKU = "BP-004", InStock = 12, Price = "$129.99", Category = "Gear" },
            ["Water Bottle"] = new { SKU = "WB-005", InStock = 45, Price = "$19.99", Category = "Hydration" },
            ["Trail Mix"] = new { SKU = "TM-006", InStock = 67, Price = "$8.99", Category = "Food" }
        };

        var searchKey = inventory.Keys.FirstOrDefault(name =>
            name.Contains(productName, StringComparison.OrdinalIgnoreCase));

        if (searchKey != null)
        {
            var result = new { Product = searchKey, Details = inventory[searchKey] };
            return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        }

        return $"Product '{productName}' not found in inventory.";
    }
}

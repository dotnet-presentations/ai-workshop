# ContosoOrders MCP Server

This sample MCP server exposes simulated order, customer, and inventory data for
the ContosoOrders business system.

## Available tools

- `get_order_details` retrieves an order by ID.
- `search_orders_by_customer` finds orders by customer name.
- `get_product_inventory` checks inventory for a product or SKU.

## Run locally

Run the server from the project directory:

```powershell
dotnet run
```

Configure an MCP client to start the project over standard input and output:

```json
{
  "servers": {
    "ContosoOrdersMcpServer": {
      "type": "stdio",
      "command": "dotnet",
      "args": [
        "run",
        "--project",
        "<PATH TO PROJECT DIRECTORY>"
      ]
    }
  }
}
```

## Create the NuGet package

Run the following command from the project directory:

```powershell
dotnet pack --configuration Release
```

The package is written to the `bin/Release` directory.

## Use the NuGet package

After publishing the package, configure an MCP client to run it with `dnx`:

```json
{
  "servers": {
    "ContosoOrdersMcpServer": {
      "type": "stdio",
      "command": "dnx",
      "args": [
        "ContosoOrdersMcpServer",
        "--version",
        "0.1.0-beta",
        "--yes"
      ]
    }
  }
}
```

For the complete workshop exercise, see
[Part 6: Enhanced MCP Server](https://github.com/dotnet-presentations/ai-workshop/blob/main/Part%2006%20-%20Enhanced%20MCP%20Server/README.md).

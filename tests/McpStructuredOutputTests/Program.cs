using System.Diagnostics;
using System.Text.Json;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;

var repositoryRoot = FindRepositoryRoot();

await BuildProjectAsync(repositoryRoot, "Part 05 - MCP Server Basics/MyMcpServer/MyMcpServer.csproj");
await BuildProjectAsync(repositoryRoot, "Part 06 - Enhanced MCP Server/ContosoOrdersMcpServer/ContosoOrdersMcpServer.csproj");
await ValidatePart5Async(repositoryRoot);
await ValidatePart6Async(repositoryRoot);

Console.WriteLine("MCP structured output validation passed.");

static async Task ValidatePart5Async(string repositoryRoot)
{
	await using var client = await CreateClientAsync(
		repositoryRoot,
		"Part 05 - MCP Server Basics/MyMcpServer/MyMcpServer.csproj");

	var tools = await client.ListToolsAsync();
	AssertToolSchema(tools, "get_current_weather", "city", "object", "city", "temperature", "condition");
	AssertToolSchema(tools, "get_weather_forecast", "city", "object", "city", "forecast");
	AssertScalarToolSchema(tools, "get_random_number");

	var weather = await client.CallToolAsync(
		"get_current_weather",
		new Dictionary<string, object?> { ["city"] = "Seattle" });
	var weatherContent = GetStructuredContent(weather, "get_current_weather");
	Assert(weatherContent.ValueKind == JsonValueKind.Object, "Weather result must be an object.");
	Assert(weatherContent.GetProperty("city").GetString() == "Seattle", "Weather city was not preserved.");

	var forecast = await client.CallToolAsync(
		"get_weather_forecast",
		new Dictionary<string, object?> { ["city"] = "Seattle" });
	var forecastContent = GetStructuredContent(forecast, "get_weather_forecast");
	Assert(forecastContent.GetProperty("forecast").GetArrayLength() == 5, "Forecast did not return five days.");

	var random = await client.CallToolAsync(
		"get_random_number",
		new Dictionary<string, object?> { ["min"] = 1, ["max"] = 2 });
	var randomContent = GetStructuredContent(random, "get_random_number");
	Assert(randomContent.ValueKind == JsonValueKind.Number, "Scalar structured content must not use a result wrapper.");
	Assert(randomContent.GetInt32() == 1, "Random number did not respect the requested range.");
}

static async Task ValidatePart6Async(string repositoryRoot)
{
	await using var client = await CreateClientAsync(
		repositoryRoot,
		"Part 06 - Enhanced MCP Server/ContosoOrdersMcpServer/ContosoOrdersMcpServer.csproj");

	var tools = await client.ListToolsAsync();
	AssertToolSchema(tools, "get_order_details", "orderId", "object", "found", "orderId");
	AssertToolSchema(tools, "search_orders_by_customer", "customerName", "object", "found", "customer", "orders");
	AssertToolSchema(tools, "get_product_inventory", "productName", "object", "found", "product");

	var order = await client.CallToolAsync(
		"get_order_details",
		new Dictionary<string, object?> { ["orderId"] = "12345" });
	var orderContent = GetStructuredContent(order, "get_order_details");
	Assert(orderContent.GetProperty("found").GetBoolean(), "Known order was not found.");
	Assert(
		orderContent.GetProperty("order").GetProperty("customer").GetString() == "John Doe",
		"Known order returned the wrong customer.");

	var customerOrders = await client.CallToolAsync(
		"search_orders_by_customer",
		new Dictionary<string, object?> { ["customerName"] = "John Doe" });
	var customerOrdersContent = GetStructuredContent(customerOrders, "search_orders_by_customer");
	Assert(customerOrdersContent.GetProperty("found").GetBoolean(), "Known customer was not found.");
	Assert(customerOrdersContent.GetProperty("orders").GetArrayLength() == 2, "Known customer returned the wrong orders.");

	var inventory = await client.CallToolAsync(
		"get_product_inventory",
		new Dictionary<string, object?> { ["productName"] = "Camping Tent" });
	var inventoryContent = GetStructuredContent(inventory, "get_product_inventory");
	Assert(inventoryContent.GetProperty("found").GetBoolean(), "Known product was not found.");
	Assert(
		inventoryContent.GetProperty("details").GetProperty("inStock").GetInt32() == 15,
		"Known product returned the wrong inventory.");

	var missingOrder = await client.CallToolAsync(
		"get_order_details",
		new Dictionary<string, object?> { ["orderId"] = "99999" });
	var missingContent = GetStructuredContent(missingOrder, "get_order_details");
	Assert(!missingContent.GetProperty("found").GetBoolean(), "Unknown order was reported as found.");
	Assert(
		!missingContent.TryGetProperty("order", out var missingOrderData) ||
			missingOrderData.ValueKind == JsonValueKind.Null,
		"Unknown order included order data.");
}

static async Task<McpClient> CreateClientAsync(string repositoryRoot, string projectPath)
{
	var environmentVariables = StdioClientTransportOptions.GetDefaultEnvironmentVariables();
	AddEnvironmentVariable(environmentVariables, "DOTNET_ROOT");
	AddEnvironmentVariable(environmentVariables, "NUGET_PACKAGES");

	var transport = new StdioClientTransport(new StdioClientTransportOptions
	{
		Name = Path.GetFileNameWithoutExtension(projectPath),
		Command = "dotnet",
		Arguments = [
			"run",
			"--project",
			Path.Combine(repositoryRoot, projectPath),
			"--configuration",
			"Release",
			"--no-build"
		],
		WorkingDirectory = repositoryRoot,
		InheritEnvironmentVariables = false,
		EnvironmentVariables = environmentVariables,
		ShutdownTimeout = TimeSpan.FromSeconds(10)
	});

	return await McpClient.CreateAsync(transport);
}

static async Task BuildProjectAsync(string repositoryRoot, string projectPath)
{
	using var process = Process.Start(new ProcessStartInfo
	{
		FileName = "dotnet",
		ArgumentList =
		{
			"build",
			Path.Combine(repositoryRoot, projectPath),
			"--configuration",
			"Release"
		},
		WorkingDirectory = repositoryRoot,
		UseShellExecute = false
	}) ?? throw new InvalidOperationException($"Could not build '{projectPath}'.");

	await process.WaitForExitAsync();
	Assert(process.ExitCode == 0, $"Build failed for '{projectPath}'.");
}

static void AssertToolSchema(
	IList<McpClientTool> tools,
	string toolName,
	string? requiredInput,
	string expectedOutputType,
	params string[] requiredOutputProperties)
{
	var tool = tools.SingleOrDefault(tool => tool.Name == toolName)
		?? throw new InvalidOperationException($"Tool '{toolName}' was not discovered.");

	var inputSchema = tool.ProtocolTool.InputSchema;
	Assert(inputSchema.GetProperty("type").GetString() == "object", $"Tool '{toolName}' has an invalid input schema.");
	if (requiredInput is not null)
	{
		Assert(
			inputSchema.GetProperty("required").EnumerateArray().Any(item => item.GetString() == requiredInput),
			$"Tool '{toolName}' does not require '{requiredInput}'.");
	}

	var outputSchema = tool.ProtocolTool.OutputSchema
		?? throw new InvalidOperationException($"Tool '{toolName}' does not advertise an output schema.");
	var actualOutputType = outputSchema.GetProperty("type").GetString();
	Assert(
		actualOutputType == expectedOutputType,
		$"Tool '{toolName}' output schema is '{actualOutputType}', not '{expectedOutputType}': {outputSchema.GetRawText()}");

	if (requiredOutputProperties.Length > 0)
	{
		var required = outputSchema.GetProperty("required");
		var properties = outputSchema.GetProperty("properties");
		foreach (var propertyName in requiredOutputProperties)
		{
			Assert(properties.TryGetProperty(propertyName, out _), $"Tool '{toolName}' has no '{propertyName}' output.");
			Assert(
				required.EnumerateArray().Any(item => item.GetString() == propertyName),
				$"Tool '{toolName}' does not require its '{propertyName}' output.");
		}
	}
}

static void AssertScalarToolSchema(IList<McpClientTool> tools, string toolName)
{
	var tool = tools.SingleOrDefault(tool => tool.Name == toolName)
		?? throw new InvalidOperationException($"Tool '{toolName}' was not discovered.");
	var outputSchema = tool.ProtocolTool.OutputSchema
		?? throw new InvalidOperationException($"Tool '{toolName}' does not advertise an output schema.");

	var isInteger = outputSchema.GetProperty("type").GetString() == "integer";
	var isWrappedInteger = outputSchema.GetProperty("type").GetString() == "object" &&
		outputSchema.GetProperty("properties").GetProperty("result").GetProperty("type").GetString() == "integer" &&
		outputSchema.GetProperty("required").EnumerateArray().Any(item => item.GetString() == "result");
	Assert(isInteger || isWrappedInteger, $"Tool '{toolName}' has an unexpected output schema: {outputSchema.GetRawText()}");
}

static JsonElement GetStructuredContent(CallToolResult result, string toolName)
{
	Assert(result.IsError is not true, $"Tool '{toolName}' returned an error.");
	return result.StructuredContent
		?? throw new InvalidOperationException($"Tool '{toolName}' did not return structured content.");
}

static void AddEnvironmentVariable(Dictionary<string, string?> environmentVariables, string name)
{
	if (Environment.GetEnvironmentVariable(name) is { } value)
	{
		environmentVariables[name] = value;
	}
}

static string FindRepositoryRoot()
{
	for (var directory = new DirectoryInfo(Directory.GetCurrentDirectory());
		 directory is not null;
		 directory = directory.Parent)
	{
		if (Directory.Exists(Path.Combine(directory.FullName, "Part 05 - MCP Server Basics")) &&
			Directory.Exists(Path.Combine(directory.FullName, "Part 06 - Enhanced MCP Server")))
		{
			return directory.FullName;
		}
	}

	throw new InvalidOperationException("Run this validation from inside the ai-workshop repository.");
}

static void Assert(bool condition, string message)
{
	if (!condition)
	{
		throw new InvalidOperationException(message);
	}
}

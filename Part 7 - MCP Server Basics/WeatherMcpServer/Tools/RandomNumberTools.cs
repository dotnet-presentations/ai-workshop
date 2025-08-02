using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol.Server;

/// <summary>
/// Weather MCP tools for demonstration purposes.
/// These tools can be invoked by MCP clients to get weather information.
/// </summary>
internal class WeatherTools
{
    [McpServerTool]
    [Description("Gets current weather for a specified city.")]
    public async Task<string> GetCurrentWeather(
        [Description("Name of the city to get weather for")] string city)
    {
        // Simulate weather API call with realistic data
        var weatherData = new
        {
            City = city,
            Temperature = Random.Shared.Next(-10, 35) + "°C",
            Condition = GetRandomWeatherCondition(),
            Humidity = Random.Shared.Next(30, 90) + "%",
            WindSpeed = Random.Shared.Next(5, 25) + " km/h",
            LastUpdated = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        };

        return JsonSerializer.Serialize(weatherData, new JsonSerializerOptions { WriteIndented = true });
    }

    [McpServerTool]
    [Description("Gets a 5-day weather forecast for a specified city.")]
    public async Task<string> GetWeatherForecast(
        [Description("Name of the city to get forecast for")] string city)
    {
        var forecast = new
        {
            City = city,
            Forecast = Enumerable.Range(1, 5).Select(day => new
            {
                Date = DateTime.Now.AddDays(day).ToString("yyyy-MM-dd"),
                HighTemp = Random.Shared.Next(15, 30) + "°C",
                LowTemp = Random.Shared.Next(-5, 15) + "°C",
                Condition = GetRandomWeatherCondition(),
                ChanceOfRain = Random.Shared.Next(0, 100) + "%"
            }).ToArray()
        };

        return JsonSerializer.Serialize(forecast, new JsonSerializerOptions { WriteIndented = true });
    }

    private static string GetRandomWeatherCondition()
    {
        var conditions = new[] { "Sunny", "Partly Cloudy", "Cloudy", "Rainy", "Thunderstorms", "Snow", "Foggy" };
        return conditions[Random.Shared.Next(conditions.Length)];
    }
}

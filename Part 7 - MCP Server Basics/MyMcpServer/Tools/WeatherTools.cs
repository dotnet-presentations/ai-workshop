using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol.Server;

namespace MyMcpServer.Tools;

/// <summary>
/// Weather tools that provide current weather and forecast information.
/// </summary>
internal class WeatherTools
{
    private static readonly string[] WeatherConditions = [
        "Sunny", "Partly Cloudy", "Cloudy", "Overcast", "Light Rain", 
        "Heavy Rain", "Snow", "Fog", "Windy", "Stormy"
    ];

    [McpServerTool]
    [Description("Gets current weather for a specified city.")]
    public async Task<string> GetCurrentWeather(
        [Description("Name of the city to get weather for")] string city)
    {
        // Simulate API call delay
        await Task.Delay(500);
        
        // Simulate weather API call with realistic data
        var weatherData = new
        {
            City = city,
            Temperature = Random.Shared.Next(-10, 35) + "°C",
            Condition = GetRandomWeatherCondition(),
            Humidity = Random.Shared.Next(30, 90) + "%",
            WindSpeed = Random.Shared.Next(5, 25) + " km/h",
            Pressure = Random.Shared.Next(980, 1040) + " hPa",
            LastUpdated = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        };

        return JsonSerializer.Serialize(weatherData, new JsonSerializerOptions { WriteIndented = true });
    }

    [McpServerTool]
    [Description("Gets a 5-day weather forecast for a specified city.")]
    public async Task<string> GetWeatherForecast(
        [Description("Name of the city to get forecast for")] string city)
    {
        // Simulate API call delay
        await Task.Delay(800);
        
        var forecast = new
        {
            City = city,
            Forecast = Enumerable.Range(0, 5).Select(day => new
            {
                Date = DateTime.Now.AddDays(day).ToString("yyyy-MM-dd"),
                DayName = DateTime.Now.AddDays(day).ToString("dddd"),
                HighTemp = Random.Shared.Next(15, 35) + "°C",
                LowTemp = Random.Shared.Next(-5, 20) + "°C",
                Condition = GetRandomWeatherCondition(),
                ChanceOfRain = Random.Shared.Next(0, 100) + "%"
            }).ToArray()
        };

        return JsonSerializer.Serialize(forecast, new JsonSerializerOptions { WriteIndented = true });
    }

    private static string GetRandomWeatherCondition()
    {
        return WeatherConditions[Random.Shared.Next(WeatherConditions.Length)];
    }
}

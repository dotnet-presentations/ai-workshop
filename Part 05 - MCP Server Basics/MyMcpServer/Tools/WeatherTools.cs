using System.ComponentModel;
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

    [McpServerTool(UseStructuredContent = true)]
    [Description("Gets current weather for a specified city.")]
    public async Task<CurrentWeather> GetCurrentWeather(
        [Description("Name of the city to get weather for")] string city)
    {
        // Simulate API call delay
        await Task.Delay(500);

        // Simulate weather API call with realistic data
        return new CurrentWeather(
            city,
            Random.Shared.Next(-10, 35) + "°C",
            GetRandomWeatherCondition(),
            Random.Shared.Next(30, 90) + "%",
            Random.Shared.Next(5, 25) + " km/h",
            Random.Shared.Next(980, 1040) + " hPa",
            DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
    }

    [McpServerTool(UseStructuredContent = true)]
    [Description("Gets a 5-day weather forecast for a specified city starting from tomorrow.")]
    public async Task<WeatherForecast> GetWeatherForecast(
        [Description("Name of the city to get forecast for")] string city)
    {
        // Simulate API call delay
        await Task.Delay(800);

        var forecast = Enumerable.Range(1, 5).Select(day => new WeatherForecastDay(
            DateTime.Now.AddDays(day).ToString("yyyy-MM-dd"),
            DateTime.Now.AddDays(day).ToString("dddd"),
            Random.Shared.Next(15, 35) + "°C",
            Random.Shared.Next(-5, 20) + "°C",
            GetRandomWeatherCondition(),
            Random.Shared.Next(0, 100) + "%")).ToArray();

        return new WeatherForecast(city, forecast);
    }

    private static string GetRandomWeatherCondition()
    {
        return WeatherConditions[Random.Shared.Next(WeatherConditions.Length)];
    }
}

internal sealed record CurrentWeather(
    string City,
    string Temperature,
    string Condition,
    string Humidity,
    string WindSpeed,
    string Pressure,
    string LastUpdated);

internal sealed record WeatherForecast(string City, WeatherForecastDay[] Forecast);

internal sealed record WeatherForecastDay(
    string Date,
    string DayName,
    string HighTemp,
    string LowTemp,
    string Condition,
    string ChanceOfRain);

using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddOpenApi();

var app = builder.Build();

app.UseCors("AllowAll");

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

var summaries = new[]
{
    "Sunny", "Mostly Sunny", "Partly Cloudy", "Cloudy", "Drizzle", "Heavy Rain", "Thunderstorm", "Snowy", "Windy", "Foggy"
};

app.MapGet("/", () => new { Message = "Weather API is online. Use /api/weather?city=cityName to fetch forecast details." });

app.MapGet("/api/weather", ([FromQuery] string? city) =>
{
    if (string.IsNullOrWhiteSpace(city))
    {
        return Results.BadRequest(new { Error = "City name is required." });
    }

    // Standardize city name
    string formattedCity = char.ToUpper(city[0]) + city.Substring(1).ToLowerInvariant();

    // Use city hash code as seed for consistent forecast per city
    int seed = formattedCity.GetHashCode();
    var rand = new Random(seed);

    // Generate weather condition index
    int summaryIndex = rand.Next(summaries.Length);
    string summary = summaries[summaryIndex];
    int currentTemp = rand.Next(-10, 42); // Celsius
    int humidity = rand.Next(20, 95); // %
    int windSpeed = rand.Next(2, 45); // km/h

    // Build 5-day forecast
    var forecast = Enumerable.Range(1, 5).Select(index =>
    {
        int tempChange = rand.Next(-4, 5);
        int dayTemp = currentTemp + tempChange;
        int forecastSummaryIndex = Math.Clamp(summaryIndex + rand.Next(-2, 3), 0, summaries.Length - 1);
        string daySummary = summaries[forecastSummaryIndex];

        return new DailyForecast(
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)).ToString("yyyy-MM-dd"),
            dayTemp,
            (int)(dayTemp * 1.8 + 32),
            daySummary,
            GetIconName(daySummary)
        );
    }).ToArray();

    var weatherDetails = new WeatherDetails(
        formattedCity,
        currentTemp,
        (int)(currentTemp * 1.8 + 32),
        summary,
        GetIconName(summary),
        humidity,
        windSpeed,
        forecast
    );

    return Results.Ok(weatherDetails);
});

string GetIconName(string summary)
{
    return summary switch
    {
        "Sunny" => "sunny",
        "Mostly Sunny" => "mostly-sunny",
        "Partly Cloudy" => "partly-cloudy",
        "Cloudy" => "cloudy",
        "Drizzle" => "drizzle",
        "Heavy Rain" => "rainy",
        "Thunderstorm" => "thunderstorm",
        "Snowy" => "snowy",
        "Windy" => "windy",
        "Foggy" => "foggy",
        _ => "unknown"
    };
}

app.Run();

record DailyForecast(string Date, int TemperatureC, int TemperatureF, string Summary, string Icon);
record WeatherDetails(string City, int TemperatureC, int TemperatureF, string Summary, string Icon, int Humidity, int WindSpeed, DailyForecast[] Forecast);

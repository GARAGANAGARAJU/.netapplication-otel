using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace ServiceB.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastBackendController : ControllerBase
{
    private static readonly string[] Summaries = 
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly ILogger<WeatherForecastBackendController> _logger;
    private readonly ActivitySource _activitySource = new("ServiceB");

    public WeatherForecastBackendController(ILogger<WeatherForecastBackendController> logger)
    {
        _logger = logger;
    }

    [HttpGet(Name = "GetWeatherForecast")]
    public IEnumerable<WeatherForecast> Get()
    {
        using var activity = _activitySource.StartActivity(nameof(GetForecast));
        return GetForecast();
    }
    
    [HttpPost(Name = "WeatherForecastRequest")]
    public string Post()
    {
        var directory = Directory.CreateDirectory(Path.Join(Directory.GetCurrentDirectory(), "forecast"));
        var filePath = Path.Join(directory.FullName, $"{Guid.NewGuid()}.json");
        using var fileStream = System.IO.File.OpenWrite(filePath);
        using var writer = new StreamWriter(fileStream);
        
        using var activity = _activitySource.StartActivity(nameof(GetForecast), ActivityKind.Producer);
        var forecast = GetForecast();
        writer.WriteLine(JsonSerializer.Serialize(forecast, new JsonSerializerOptions(JsonSerializerDefaults.Web)));
        _logger.LogInformation("Forecast written to {filePath}", filePath);
        activity?.SetTag("filePath", filePath);

        return filePath;
    }

    private static IReadOnlyCollection<WeatherForecast> GetForecast()
    {
        return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTimeOffset.Now.AddDays(index),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
    }
}

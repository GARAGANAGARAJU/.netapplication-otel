using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace ServiceA.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastFrontendController : ControllerBase
{
    private readonly ILogger<WeatherForecastFrontendController> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ActivitySource _activitySource = new("ServiceA");

    public WeatherForecastFrontendController(
        ILogger<WeatherForecastFrontendController> logger,
        IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    [HttpGet(Name = "GetWeatherForecast")]
    public async Task<IEnumerable<WeatherForecast>?> Get()
    {
        var httpClient = _httpClientFactory.CreateClient("backend");
        using var responseMessage = await httpClient.GetAsync("WeatherForecastBackend");
        if (responseMessage.IsSuccessStatusCode)
        {
            return await responseMessage.Content.ReadFromJsonAsync<IEnumerable<WeatherForecast>?>();
        }

        return null;
    }
    
    [HttpPost(Name = "WeatherForecastRequest")]
    public async Task<IEnumerable<WeatherForecast>?> Post()
    {
        var httpClient = _httpClientFactory.CreateClient("backend");
        using var responseMessage = await httpClient.PostAsync("WeatherForecastBackend", new StringContent(string.Empty));
        if (responseMessage.IsSuccessStatusCode)
        {
            var filePath = await responseMessage.Content.ReadAsStringAsync();
            _logger.LogInformation("Reading forecast from {filePath}", filePath);

            using var activity = _activitySource.StartActivity("ProcessForecast", ActivityKind.Consumer);
            var json = await System.IO.File.ReadAllTextAsync(filePath);
            return JsonSerializer.Deserialize<IEnumerable<WeatherForecast>?>(json, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        }

        return null;
    }
}
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using System.Text;
using System.Text.Json;

namespace Api.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<WeatherForecastController> _logger;
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };


    public WeatherForecastController(IDistributedCache cache, ILogger<WeatherForecastController> logger)
    {
        _cache = cache;
        _logger = logger;
    }


    [HttpGet(Name = "GetWeatherForecast")]
    public async Task<IEnumerable<WeatherForecast>> Get()
    {
        var result = await _cache.GetAsync("key");

        if (result is not null)
            return JsonSerializer.Deserialize<IEnumerable<WeatherForecast>>(result);
        
        var data = Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        })
        .ToArray();
        var jsonData = JsonSerializer.Serialize(data);
        byte[] encodedData = Encoding.UTF8.GetBytes(jsonData);

        var options = new DistributedCacheEntryOptions()
                          .SetSlidingExpiration(TimeSpan.FromSeconds(30));
        await _cache.SetAsync("key", encodedData, options);
        return data;
    }
}

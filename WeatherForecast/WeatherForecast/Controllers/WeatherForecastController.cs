using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using System;

namespace WeatherForecast.Controllers
{
	public class Body
	{
        public string Name { get; set; }
		public string Surname { get; set; }
    }

	//[Authorize]
	[ApiController]
	[Route("[controller]/[action]")]
	public class WeatherForecastController : ControllerBase
	{
		private static readonly string[] Summaries = new[]
		{
		"Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
	};

		private readonly ILogger<WeatherForecastController> _logger;

		public WeatherForecastController(ILogger<WeatherForecastController> logger)
		{
			_logger = logger;
		}

		[HttpGet(Name = "GetWeatherForecast")]
		public IEnumerable<WeatherForecast> Get()
		{
			// For Response-Cache
			//Response.Headers[HeaderNames.CacheControl] = "no-cache";
			//Response.Headers[HeaderNames.CacheControl] = "no-transform";

			return Enumerable.Range(1, 1).Select(index => new WeatherForecast
			{
				Date = DateTime.Now.AddDays(index),
				TemperatureC = Random.Shared.Next(-20, 55),
				Summary = Summaries[Random.Shared.Next(Summaries.Length)]
			})
			.ToArray();
		}

		[HttpPost]
		public IEnumerable<WeatherForecast> Post([FromBody] Body body)
		{
			if (HttpContext.Request.ContentType != null && HttpContext.Request.ContentType.Contains("application/json"))
				throw new InvalidDataException("Invalid request message format . It must be in application/xml !");

			Console.WriteLine("\nBody :- " + body.Name);
			foreach (var header in HttpContext.Request.Headers)
			{
				Console.WriteLine(header.Key + " : " + header.Value);
			}

			int total = 10;
			if (body.Name == "ronak")
				total = 1;

			return Enumerable.Range(1, total).Select(index => new WeatherForecast
			{
				Date = DateTime.Now.AddDays(index),
				TemperatureC = Random.Shared.Next(-20, 55),
				Summary = Summaries[Random.Shared.Next(Summaries.Length)]
			})
			.ToArray();
		}
	}
}
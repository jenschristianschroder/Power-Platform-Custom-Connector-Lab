using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace CompanyName.Weather
{
    public static class GetWeather
    {
        [FunctionName("GetWeather")]
        [OpenApiOperation(operationId: "Run", tags: new[] { "name" })]
        [OpenApiParameter(name: "name", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "The **Name** parameter")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "The OK response")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string name = req.Query["name"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            name = name ?? data?.name;

            // Weather Forecast Object
            weatherForecast weatherForecastObject = new weatherForecast();
            weatherForecastObject.locations = new List<location> {
                new location { locationName = "Bergen", forecast = "Rain" },
                new location { locationName = "Oslo", forecast = "Cloudy" },
                new location { locationName = "Stavanger", forecast = "Sunny" }
            };
            
            // Find location matching querystring parameter name
            var forecast = weatherForecastObject.locations.Find(x => x.locationName.ToLower() == name.ToLower());
            
            // if location matching querystring parameter name is not found return error message 
            if(forecast == null) {
                forecast = new location { locationName = name, forecast = $"Location '{name}' not found" };
            } 
            // return weather forecast for location
            var jsonForecast = JsonConvert.SerializeObject(forecast);
            return new OkObjectResult(jsonForecast);
        }
    }

    public class weatherForecast {
        public List<location> locations { get; set; }
    }
    public class location {
        public string locationName { get; set; }
        public string forecast { get; set; }
    }
}
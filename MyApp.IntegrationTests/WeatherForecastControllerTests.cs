using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MyApp.IntegrationTests
{
    [TestClass]
    public class WeatherForecastControllerTests
    {
        private static WebApplicationFactory<Startup> _factory;

        [ClassInitialize]
        public static void ClassInit(TestContext testContext)
        {
            Console.WriteLine(testContext.TestName);
            _factory = new WebApplicationFactory<Startup>();
        }

        [TestMethod]
        public async Task ShouldReturnSuccessResponse()
        {
            var client = _factory.CreateClient();
            var response = await client.GetAsync("WeatherForecast");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("application/json; charset=utf-8", response.Content.Headers.ContentType?.ToString());

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<WeatherForecast[]>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            Assert.AreEqual(5, result?.Length);
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            _factory.Dispose();
        }
    }
}

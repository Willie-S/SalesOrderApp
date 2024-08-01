using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace SalesOrderApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DogApiController : ControllerBase
    {
        private readonly HttpClient _httpClient;

        public DogApiController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // GET: api/Dog/RandomImage
        [HttpGet("RandomImage")]
        public async Task<IActionResult> GetRandomDogImage()
        {
            var response = await _httpClient.GetAsync("https://dog.ceo/api/breeds/image/random");
            if (!response.IsSuccessStatusCode)
            {
                return StatusCode((int)response.StatusCode, "Error fetching dog image.");
            }

            var content = await response.Content.ReadAsStringAsync();
            var dogImageResponse = JsonConvert.DeserializeObject<DogImageResponse>(content);

            if (dogImageResponse == null || string.IsNullOrEmpty(dogImageResponse.Message))
            {
                return BadRequest("Invalid response from Dog API.");
            }

            return Ok(dogImageResponse.Message);
        }

        public class DogImageResponse
        {
            public string Message { get; set; }
            public string Status { get; set; }
        }
    }
}

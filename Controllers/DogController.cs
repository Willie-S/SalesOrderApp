using Microsoft.AspNetCore.Mvc;
using SalesOrderApp.ViewModels;

namespace SalesOrderApp.Controllers
{
    public class DogController : Controller
    {
        private readonly HttpClient _httpClient;

        public DogController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IActionResult> Index()
        {
            var response = await _httpClient.GetAsync("https://localhost:7065/api/DogApi/RandomImage");
            if (!response.IsSuccessStatusCode)
            {
                // Handle error here (e.g., set an error message in the view model)
                return View(new DogViewModel());
            }

            var imageUrl = await response.Content.ReadAsStringAsync();
            var viewModel = new DogViewModel { ImageUrl = imageUrl };

            return View(viewModel);
        }
    }
}

// Controllers/HomeController.cs
using System.Diagnostics;
using ABCRetailers.Models;
using ABCRetailers.Models.ViewModels;
using ABCRetailers.Services.FunctionsApi;
using Microsoft.AspNetCore.Mvc;
using ABCRetailers.Models.FunctionsDtos;

namespace ABCRetailers.Controllers
{
    public class HomeController : Controller
    {
        private readonly IFunctionsApi _api;
        private readonly ILogger<HomeController> _logger;

        public HomeController(IFunctionsApi api, ILogger<HomeController> logger)
        {
            _api = api;
            _logger = logger;
        }

        // ---------------- Index ----------------
        public async Task<IActionResult> Index()
        {
            try
            {
                var products = await _api.GetProductsAsync() ?? new List<ProductDto>();
                var customers = await _api.GetCustomersAsync() ?? new List<CustomerDto>();
                var orders = await _api.GetOrdersAsync() ?? new List<OrderDto>();

                var viewModelDto = new HomeViewModelDto
                {
                    FeaturedProducts = products.Take(5).ToList(),
                    ProductCount = products.Count,
                    CustomerCount = customers.Count,
                    OrderCount = orders.Count
                };

                return View(viewModelDto); // ✅ now matches your view's @model
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading home dashboard.");
                TempData["Error"] = "Failed to load dashboard data.";

                // return empty DTO instead of old ViewModel
                return View(new HomeViewModelDto());
            }
        }

        // ---------------- Privacy ----------------
        public IActionResult Privacy()
        {
            return View();
        }

        // ---------------- Error ----------------
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

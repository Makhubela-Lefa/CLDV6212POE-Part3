using System.Diagnostics;
using ABCRetailers.Models;
using ABCRetailers.Services;
using ABCRetailers.Models.ViewModels;
using ABCRetailers.Services.FunctionsApi;
using Microsoft.AspNetCore.Authorization;
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

        // ---------------- MAIN HOME PAGE (PUBLIC) ----------------
        [AllowAnonymous]
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

                return View(viewModelDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading home dashboard.");
                TempData["Error"] = "Failed to load dashboard data.";
                return View(new HomeViewModelDto());
            }
        }

        // ---------------- ADMIN HOME PAGE ----------------
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminHome()
        {
            try
            {
                var customers = await _api.GetCustomersAsync() ?? new List<CustomerDto>();
                var orders = await _api.GetOrdersAsync() ?? new List<OrderDto>();
                var products = await _api.GetProductsAsync() ?? new List<ProductDto>();

                var viewModelDto = new HomeViewModelDto
                {
                    ProductCount = products.Count,
                    CustomerCount = customers.Count,
                    OrderCount = orders.Count,
                    FeaturedProducts = products.Take(3).ToList()
                };

                ViewBag.Message = "Welcome Admin 👑";
                return View("AdminHome", viewModelDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading Admin Home page.");
                TempData["Error"] = "Failed to load Admin dashboard.";
                return RedirectToAction("Index");
            }
        }

        // ---------------- CUSTOMER HOME PAGE ----------------
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> CustomerHome()
        {
            try
            {
                var username = HttpContext.Session.GetString("Username") ?? "Guest";
                var products = await _api.GetProductsAsync() ?? new List<ProductDto>();

                ViewBag.Username = username;
                ViewBag.Message = "Welcome to your dashboard 🛍️";

                // Featured products = first 3 items
                ViewBag.FeaturedProducts = products.Take(3).ToList();

                // Return only main product grid (6 items)
                return View("CustomerHome", products.Take(6).ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading Customer Home page.");
                TempData["Error"] = "Failed to load Customer page.";
                return RedirectToAction("Index");
            }
        }


        // ---------------- Privacy ----------------
        [AllowAnonymous]
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

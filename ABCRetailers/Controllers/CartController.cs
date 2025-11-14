using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ABCRetailers.Data;
using ABCRetailers.Models;
using ABCRetailers.Models.FunctionsDtos;
using ABCRetailers.Models.ViewModels;
using ABCRetailers.Services.FunctionsApi;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ABCRetailers.Controllers
{
    [Authorize(Roles = "Customer")] // only logged-in customers can access
    public class CartController : Controller
    {
        private readonly AuthDbContext _db;
        private readonly IFunctionsApi _api;

        public CartController(AuthDbContext db, IFunctionsApi api)
        {
            _db = db;
            _api = api;
        }

        // ========================================================
        // GET: /Cart/Index - Display items in cart
        // ========================================================
        public async Task<IActionResult> Index()
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Index", "Login");

            var cartItems = await _db.Cart
                .Where(c => c.CustomerUsername == username)
                .ToListAsync();

            var viewModelList = new List<CartItemViewModel>();

            foreach (var cartItem in cartItems)
            {
                var product = await _api.GetProductAsync(cartItem.ProductId);
                if (product == null) continue;

                viewModelList.Add(new CartItemViewModel
                {
                    ProductId = product.Id,
                    ProductName = product.ProductName,
                    Quantity = cartItem.Quantity,
                    UnitPrice = (decimal)product.Price
                });
            }

            return View(new CartPageViewModel { Items = viewModelList });
        }

        // ========================================================
        // GET: /Cart/Add - Add product to cart
        // ========================================================
        public async Task<IActionResult> Add(string productId)
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(productId))
                return RedirectToAction("Index", "Product");

            var product = await _api.GetProductAsync(productId);
            if (product == null)
                return NotFound();

            var existing = await _db.Cart.FirstOrDefaultAsync(c =>
                c.ProductId == productId && c.CustomerUsername == username);

            if (existing != null)
            {
                existing.Quantity += 1;
            }
            else
            {
                _db.Cart.Add(new Cart
                {
                    CustomerUsername = username,
                    ProductId = productId,
                    Quantity = 1
                });
            }

            await _db.SaveChangesAsync();
            TempData["Success"] = $"{product.ProductName} added to cart.";
            return RedirectToAction("Index", "Product");
        }

        // ========================================================
        // POST: /Cart/Checkout - Place orders for all items in cart
        // ========================================================
        [HttpPost]
        public async Task<IActionResult> Checkout()
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Index", "Login");

            // Step 1: Get customer from Azure Functions
            var customers = await _api.GetCustomersAsync();
            var customer = customers.FirstOrDefault(c => c.Username == username);
            if (customer == null)
            {
                TempData["Error"] = "Customer not found.";
                return RedirectToAction("Index");
            }

            // Step 2: Get all cart items from local DB
            var cartItems = await _db.Cart
                .Where(c => c.CustomerUsername == username)
                .ToListAsync();

            if (!cartItems.Any())
            {
                TempData["Error"] = "Your cart is empty.";
                return RedirectToAction("Index");
            }

            // Step 3: Create Azure orders for each item
            foreach (var cartItem in cartItems)
            {
                var orderDto = new CreateOrderDto
                {
                    CustomerId = customer.Id,
                    ProductId = cartItem.ProductId,
                    Quantity = cartItem.Quantity
                };

                await _api.CreateOrderAsync(orderDto);
            }

            // Step 4: Clear cart
            _db.Cart.RemoveRange(cartItems);
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = "Order placed successfully!";
            return RedirectToAction("Confirmation");
        }

        // ========================================================
        // GET: /Cart/Confirmation - Confirmation after checkout
        // ========================================================
        public IActionResult Confirmation()
        {
            ViewBag.Message = TempData["SuccessMessage"] ?? "Thank you for your purchase!";
            return View();
        }

        // ========================================================
        // POST: /Cart/Remove - Remove product from cart
        // ========================================================
        [HttpPost]
        public async Task<IActionResult> Remove(string productId)
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username)) return RedirectToAction("Index");

            var item = await _db.Cart.FirstOrDefaultAsync(c =>
                c.CustomerUsername == username && c.ProductId == productId);

            if (item != null)
            {
                _db.Cart.Remove(item);
                await _db.SaveChangesAsync();
                TempData["Success"] = "Item removed from cart.";
            }

            return RedirectToAction("Index");
        }

        // ========================================================
        // POST: /Cart/UpdateQuantities - Update quantities in cart
        // ========================================================
        [HttpPost]
        public async Task<IActionResult> UpdateQuantities(List<CartItemViewModel> items)
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username)) return RedirectToAction("Index");

            foreach (var item in items)
            {
                var cartItem = await _db.Cart.FirstOrDefaultAsync(c =>
                    c.CustomerUsername == username && c.ProductId == item.ProductId);

                if (cartItem != null)
                {
                    cartItem.Quantity = item.Quantity;
                }
            }

            await _db.SaveChangesAsync();
            TempData["Success"] = "Cart updated successfully.";
            return RedirectToAction("Index");
        }
    }
}

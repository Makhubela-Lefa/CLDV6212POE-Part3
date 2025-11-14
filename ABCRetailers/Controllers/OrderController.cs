using System;
using System.Linq;
using System.Threading.Tasks;
using ABCRetailers.Models.FunctionsDtos;
using ABCRetailers.Models.ViewModels;
using ABCRetailers.Services.FunctionsApi;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ABCRetailers.Controllers
{
    [Authorize] // All actions require authentication
    public class OrderController : Controller
    {
        private readonly IFunctionsApi _api;

        public OrderController(IFunctionsApi api)
        {
            _api = api;
        }

        // ========================================================
        // ADMIN ONLY
        // ========================================================

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var orders = await _api.GetOrdersAsync();
            return View(orders);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var order = await _api.GetOrderAsync(id);
            if (order == null) return NotFound();

            var model = new OrderCreateViewModel
            {
                CustomerId = order.CustomerId,
                ProductId = order.ProductId,
                Quantity = order.Quantity,
                Status = order.Status,
                OrderDate = DateTime.TryParse(order.OrderDate, out var dt) ? dt : DateTime.Now
            };

            await PopulateDropdowns(model);
            return View(model);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, OrderCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateDropdowns(model);
                return View(model);
            }

            try
            {
                var updateDto = new UpdateOrderStatusDto { NewStatus = model.Status };
                await _api.UpdateOrderStatusAsync(id, updateDto);
                TempData["Success"] = "Order updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error updating order: {ex.Message}");
                await PopulateDropdowns(model);
                return View(model);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            try
            {
                await _api.DeleteOrderAsync(id);
                TempData["Success"] = "Order deleted successfully!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error deleting order: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> UpdateOrderStatus([FromBody] UpdateOrderStatusRequest request)
        {
            if (string.IsNullOrEmpty(request.Id) || string.IsNullOrEmpty(request.NewStatus))
                return Json(new { success = false, message = "Invalid request." });

            try
            {
                var updateDto = new UpdateOrderStatusDto { NewStatus = request.NewStatus };
                await _api.UpdateOrderStatusAsync(request.Id, updateDto);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        public class UpdateOrderStatusRequest
        {
            public required string Id { get; set; }
            public required string NewStatus { get; set; }
        }

        // ========================================================
        // CUSTOMER ONLY
        // ========================================================

        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> CustomerOrders()
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username)) return RedirectToAction("Index", "Login");

            var allOrders = await _api.GetOrdersAsync();
            var customerOrders = allOrders.Where(o => o.Username == username).ToList();

            return View(customerOrders);
        }

        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var order = await _api.GetOrderAsync(id);
            if (order == null) return NotFound();

            var username = User.Identity?.Name;
            if (order.Username != username) return Forbid();

            return View(order);
        }

        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> Create()
        {
            var viewModel = new OrderCreateViewModel
            {
                Customers = await _api.GetCustomersAsync(),
                Products = await _api.GetProductsAsync()
            };
            return View(viewModel);
        }

        [Authorize(Roles = "Customer")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(OrderCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateDropdowns(model);
                return View(model);
            }

            try
            {
                var createDto = new CreateOrderDto
                {
                    CustomerId = model.CustomerId,
                    ProductId = model.ProductId,
                    Quantity = model.Quantity
                };

                var orderId = await _api.CreateOrderAsync(createDto);
                if (orderId != null)
                {
                    TempData["Success"] = "Order created successfully!";
                    return RedirectToAction(nameof(CustomerOrders));
                }

                ModelState.AddModelError("", "Failed to create order.");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error creating order: {ex.Message}");
            }

            await PopulateDropdowns(model);
            return View(model);
        }

        // ========================================================
        // UTILITIES
        // ========================================================

        private async Task PopulateDropdowns(OrderCreateViewModel model)
        {
            model.Customers = await _api.GetCustomersAsync();
            model.Products = await _api.GetProductsAsync();
        }
    }
}

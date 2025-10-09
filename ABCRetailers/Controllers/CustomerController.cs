using ABCRetailers.Models;
using ABCRetailers.Models.FunctionsDtos;
using ABCRetailers.Services.FunctionsApi;
using Microsoft.AspNetCore.Mvc;

namespace ABCRetailers.Controllers
{
    public class CustomerController : Controller
    {
        private readonly IFunctionsApi _api;

        public CustomerController(IFunctionsApi api)
        {
            _api = api;
        }

        // ---------------- Index ----------------
        public async Task<IActionResult> Index()
        {
            var customers = await _api.GetCustomersAsync() ?? new List<CustomerDto>();
            return View(customers);
        }

        // ---------------- Create (GET) ----------------
        public IActionResult Create()
        {
            return View();
        }

        // ---------------- Create (POST) ----------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateCustomerDto customer)
        {
            if (!ModelState.IsValid)
                return View(customer);

            try
            {
                var id = await _api.CreateCustomerAsync(customer);
                if (id != null)
                {
                    TempData["Success"] = "Customer created successfully!";
                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError("", "Failed to create customer.");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error creating customer: {ex.Message}");
            }

            return View(customer);
        }

        // ---------------- Edit (GET) ----------------
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            var customer = await _api.GetCustomerAsync(id);
            if (customer == null)
                return NotFound();

            // Map to Update DTO
            var updateDto = new UpdateCustomerDto
            {
                Name = customer.Name,
                Surname = customer.Surname,
                Email = customer.Email,
                Username = customer.Username,
                ShippingAddress = customer.ShippingAddress
            };

            ViewBag.CustomerId = id;
            return View(updateDto);
        }

        // ---------------- Edit (POST) ----------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, UpdateCustomerDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            try
            {
                await _api.UpdateCustomerAsync(id, dto);
                TempData["Success"] = "Customer updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error updating customer: {ex.Message}");
                return View(dto);
            }
        }

        // ---------------- Delete ----------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                await _api.DeleteCustomerAsync(id);
                TempData["Success"] = "Customer deleted successfully!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error deleting customer: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}

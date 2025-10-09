using ABCRetailers.Models.FunctionsDtos;
using ABCRetailers.Services.FunctionsApi;
using Microsoft.AspNetCore.Mvc;

namespace ABCRetailers.Controllers
{
    public class ProductController : Controller
    {
        private readonly IFunctionsApi _api;
        private readonly ILogger<ProductController> _logger;

        public ProductController(IFunctionsApi api, ILogger<ProductController> logger)
        {
            _api = api;
            _logger = logger;
        }

        // ----------------- Index -----------------
        public async Task<IActionResult> Index()
        {
            var products = await _api.GetProductsAsync(); // Returns List<ProductDto>
            return View(products);
        }

        // ----------------- Create (GET) -----------------
        public IActionResult Create()
        {
            return View();
        }

        // ----------------- Create (POST) -----------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateProductDto dto, IFormFile? imageFile)
        {
            if (!ModelState.IsValid)
                return View(dto);

            try
            {
                // Create product via API
                var productId = await _api.CreateProductAsync(dto);

                // Upload image if provided
                if (imageFile != null && imageFile.Length > 0)
                {
                    await _api.UploadProofAsync(imageFile, productId, dto.ProductName);
                }

                TempData["Success"] = $"Product '{dto.ProductName}' created successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product");
                ModelState.AddModelError("", $"Error creating product: {ex.Message}");
                return View(dto);
            }
        }

        // ----------------- Edit (GET) -----------------
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var product = await _api.GetProductAsync(id); // Returns ProductDto
            if (product == null) return NotFound();

            // Map ProductDto to UpdateProductDto
            var updateDto = new UpdateProductDto
            {
                Id = product.Id,
                ProductName = product.ProductName,
                Description = product.Description,
                Price = product.Price,
                StockAvailable = product.StockAvailable,
                ImageUrl = product.ImageUrl
            };

            return View(updateDto);
        }

        // ----------------- Edit (POST) -----------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UpdateProductDto updateDto, IFormFile? imageFile)
        {
            if (!ModelState.IsValid)
                return View(updateDto);

            try
            {
                // Upload new image if provided
                string? imageUrl = updateDto.ImageUrl;
                if (imageFile != null && imageFile.Length > 0)
                {
                    imageUrl = await _api.UploadProofAsync(imageFile, updateDto.Id, updateDto.ProductName);
                }

                updateDto.ImageUrl = imageUrl;

                await _api.UpdateProductAsync(updateDto.Id, updateDto);

                TempData["Success"] = $"Product '{updateDto.ProductName}' updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product: {Message}", ex.Message);
                ModelState.AddModelError("", $"Error updating product: {ex.Message}");
                return View(updateDto);
            }
        }

        // ----------------- Delete -----------------
        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                await _api.DeleteProductAsync(id);
                TempData["Success"] = "Product deleted successfully!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error deleting product: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}

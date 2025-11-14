using ABCRetailers.Models.FunctionsDtos;
using ABCRetailers.Services.FunctionsApi;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ABCRetailers.Controllers
{
    [Authorize] //protects all actions in this controller
    public class ProductController : Controller
    {
        private readonly IFunctionsApi _api;
        private readonly ILogger<ProductController> _logger;

        public ProductController(IFunctionsApi api, ILogger<ProductController> logger)
        {
            _api = api;
            _logger = logger;
        }

        //both admin and customer can view products
        // ----------------- Index -----------------
        [Authorize(Roles ="Admin,Customer")]
        public async Task<IActionResult> Index()
        {
            var products = await _api.GetProductsAsync(); // Returns List<ProductDto>
            return View(products);
        }

        //only admin can create a product
        // ----------------- Create (GET) -----------------
        [Authorize(Roles ="Admin")]
        public IActionResult Create() => View();

        //only admin can post a product
        // ----------------- Create (POST) -----------------
        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(CreateProductDto dto, IFormFile? imageFile)
        {
            if (!ModelState.IsValid)
                return View(dto);

            try
            {
                // 1. Create product
                var productId = await _api.CreateProductAsync(dto);

                // 2. Upload image if provided
                if (imageFile != null && imageFile.Length > 0)
                {
                    var imageUrl = await _api.UploadProductImageAsync(imageFile, productId);

                    if (!string.IsNullOrEmpty(imageUrl))
                    {
                        // 3. Update product with image URL
                        var updateDto = new UpdateProductDto
                        {
                            Id = productId,
                            ProductName = dto.ProductName,
                            Description = dto.Description,
                            Price = dto.Price,
                            StockAvailable = dto.StockAvailable,
                            ImageUrl = imageUrl
                        };
                        await _api.UpdateProductAsync(productId, updateDto);
                    }
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

        //only admin can edit a product
        // ----------------- Edit (GET) -----------------
        [Authorize(Roles ="Admin")]
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

        //only an admin can POST an edit
        // ----------------- Edit (POST) -----------------
        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(UpdateProductDto updateDto, IFormFile? imageFile)
        {
            if (!ModelState.IsValid)
                return View(updateDto);

            try
            {
                // Keep existing image unless a new one is uploaded
                if (imageFile != null && imageFile.Length > 0)
                {
                    var imageUrl = await _api.UploadProductImageAsync(imageFile, updateDto.Id);
                    if (!string.IsNullOrEmpty(imageUrl))
                        updateDto.ImageUrl = imageUrl;
                }

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


        //only admin can delete a product
        // ----------------- Delete -----------------
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                // 1. Check if product exists
                var product = await _api.GetProductAsync(id);
                if (product == null)
                {
                    TempData["Error"] = "Product not found.";
                    return RedirectToAction(nameof(Index));
                }

                // 2. Proceed with delete
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

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using ABCRetailers.Models.FunctionsDtos;

namespace ABCRetailers.Services.FunctionsApi
{
    public interface IFunctionsApi
    {
        // Customers
        Task<List<CustomerDto>> GetCustomersAsync();
        Task<CustomerDto?> GetCustomerAsync(string id);
        Task<string?> CreateCustomerAsync(CreateCustomerDto dto);
        Task<string?> GetCustomerByUsernameAsync(string Username); //add
        Task UpdateCustomerAsync(string id, UpdateCustomerDto dto);
        Task DeleteCustomerAsync(string id);

        // Products
        Task<List<ProductDto>> GetProductsAsync();
        Task<ProductDto?> GetProductAsync(string id);
        Task<string?> CreateProductAsync(CreateProductDto dto);
        Task UpdateProductAsync(string id, UpdateProductDto dto);
        Task DeleteProductAsync(string id);

        // Orders
        Task<List<OrderDto>> GetOrdersAsync();
        Task<OrderDto?> GetOrderAsync(string id);
        Task<string?> CreateOrderAsync(CreateOrderDto dto);
        Task UpdateOrderStatusAsync(string id, UpdateOrderStatusDto dto);
        Task DeleteOrderAsync(string id);

        // Uploads
        Task<string?> UploadProofAsync(IFormFile file, string? orderId = null, string? customerName = null);
        Task<string?> UploadFileAsync(string relativeUrl, Stream fileStream, string fileName);
        Task<string?> UploadProductImageAsync(IFormFile file, string productId);

    }
}

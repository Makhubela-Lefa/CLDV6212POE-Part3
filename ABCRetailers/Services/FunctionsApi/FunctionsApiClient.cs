using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Headers;
using ABCRetailers.Models.FunctionsDtos;

namespace ABCRetailers.Services.FunctionsApi
{
    public class FunctionsApiClient : IFunctionsApi
    {
        private readonly HttpClient _client;
        private readonly ILogger<FunctionsApiClient> _logger;

        public FunctionsApiClient(IHttpClientFactory httpFactory, ILogger<FunctionsApiClient> logger)
        {
            _client = httpFactory.CreateClient("Functions"); // Named client configured in Program.cs
            _logger = logger;
        }

        // ---------------- Customers ----------------
        public async Task<List<CustomerDto>> GetCustomersAsync()
        {
            try
            {
                var resp = await _client.GetAsync("customers");
                if (resp.StatusCode == HttpStatusCode.NotFound) return new List<CustomerDto>();
                resp.EnsureSuccessStatusCode();
                var list = await resp.Content.ReadFromJsonAsync<List<CustomerDto>>();
                return list ?? new List<CustomerDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetCustomersAsync failed");
                return new List<CustomerDto>();
            }
        }

        public async Task<CustomerDto?> GetCustomerAsync(string id)
        {
            try
            {
                var resp = await _client.GetAsync($"customers/{id}");
                if (resp.StatusCode == HttpStatusCode.NotFound) return null;
                resp.EnsureSuccessStatusCode();
                return await resp.Content.ReadFromJsonAsync<CustomerDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetCustomerAsync failed for id {Id}", id);
                return null;
            }
        }

        public async Task<string?> CreateCustomerAsync(CreateCustomerDto dto)
        {
            try
            {
                var resp = await _client.PostAsJsonAsync("customers", dto);
                resp.EnsureSuccessStatusCode();
                var obj = await resp.Content.ReadFromJsonAsync<Dictionary<string, string?>>();
                if (obj != null && obj.TryGetValue("id", out var id)) return id;
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CreateCustomerAsync failed");
                return null;
            }
        }

        public async Task UpdateCustomerAsync(string id, UpdateCustomerDto dto)
        {
            try
            {
                var resp = await _client.PutAsJsonAsync($"customers/{id}", dto);
                resp.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpdateCustomerAsync failed for id {Id}", id);
                throw;
            }
        }

        public async Task DeleteCustomerAsync(string id)
        {
            try
            {
                var resp = await _client.DeleteAsync($"customers/{id}");
                if (resp.StatusCode == HttpStatusCode.NotFound) return;
                resp.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DeleteCustomerAsync failed for id {Id}", id);
                throw;
            }
        }

        // ---------------- Products ----------------
        public async Task<List<ProductDto>> GetProductsAsync()
        {
            try
            {
                var resp = await _client.GetAsync("products");
                resp.EnsureSuccessStatusCode();
                var list = await resp.Content.ReadFromJsonAsync<List<ProductDto>>();
                return list ?? new List<ProductDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetProductsAsync failed");
                return new List<ProductDto>();
            }
        }

        public async Task<ProductDto?> GetProductAsync(string id)
        {
            try
            {
                var resp = await _client.GetAsync($"products/{id}");
                if (resp.StatusCode == HttpStatusCode.NotFound) return null;
                resp.EnsureSuccessStatusCode();
                return await resp.Content.ReadFromJsonAsync<ProductDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetProductAsync failed for id {Id}", id);
                return null;
            }
        }

        public async Task<string?> CreateProductAsync(CreateProductDto dto)
        {
            try
            {
                var resp = await _client.PostAsJsonAsync("products", dto);
                resp.EnsureSuccessStatusCode();
                var obj = await resp.Content.ReadFromJsonAsync<Dictionary<string, string?>>();
                if (obj != null && obj.TryGetValue("id", out var id)) return id;
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CreateProductAsync failed");
                return null;
            }
        }

        public async Task UpdateProductAsync(string id, UpdateProductDto dto)
        {
            try
            {
                var resp = await _client.PutAsJsonAsync($"products/{id}", dto);
                resp.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpdateProductAsync failed for id {Id}", id);
                throw;
            }
        }

        public async Task DeleteProductAsync(string id)
        {
            try
            {
                var resp = await _client.DeleteAsync($"products/{id}");
                if (resp.StatusCode == HttpStatusCode.NotFound) return;
                resp.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DeleteProductAsync failed for id {Id}", id);
                throw;
            }
        }

        // ---------------- Orders ----------------
        public async Task<List<OrderDto>> GetOrdersAsync()
        {
            try
            {
                var resp = await _client.GetAsync("orders");
                resp.EnsureSuccessStatusCode();
                var list = await resp.Content.ReadFromJsonAsync<List<OrderDto>>();
                return list ?? new List<OrderDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetOrdersAsync failed");
                return new List<OrderDto>();
            }
        }

        public async Task<OrderDto?> GetOrderAsync(string id)
        {
            try
            {
                var resp = await _client.GetAsync($"orders/{id}");
                if (resp.StatusCode == HttpStatusCode.NotFound) return null;
                resp.EnsureSuccessStatusCode();
                return await resp.Content.ReadFromJsonAsync<OrderDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetOrderAsync failed for id {Id}", id);
                return null;
            }
        }

        public async Task<string?> CreateOrderAsync(CreateOrderDto dto)
        {
            try
            {
                var resp = await _client.PostAsJsonAsync("orders", dto);
                resp.EnsureSuccessStatusCode();
                var obj = await resp.Content.ReadFromJsonAsync<Dictionary<string, string?>>();
                if (obj != null && obj.TryGetValue("id", out var id)) return id;
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CreateOrderAsync failed");
                return null;
            }
        }

        public async Task UpdateOrderStatusAsync(string id, UpdateOrderStatusDto dto)
        {
            try
            {
                var resp = await _client.PostAsJsonAsync($"orders/{id}/status", dto);
                resp.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpdateOrderStatusAsync failed for id {Id}", id);
                throw;
            }
        }

        public async Task DeleteOrderAsync(string id)
        {
            try
            {
                var resp = await _client.DeleteAsync($"orders/{id}");
                if (resp.StatusCode == HttpStatusCode.NotFound) return;
                resp.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DeleteOrderAsync failed for id {Id}", id);
                throw;
            }
        }

        // ---------------- Multipart Upload ----------------
        public async Task<string?> UploadFileAsync(string relativeUrl, Stream fileStream, string fileName)
        {
            try
            {
                using var form = new MultipartFormDataContent();
                var fileContent = new StreamContent(fileStream);
                fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
                form.Add(fileContent, "file", fileName);

                var resp = await _client.PostAsync(relativeUrl, form);
                resp.EnsureSuccessStatusCode();

                var obj = await resp.Content.ReadFromJsonAsync<Dictionary<string, string?>>();
                if (obj != null && obj.TryGetValue("url", out var url)) return url;

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UploadFileAsync failed for {FileName}", fileName);
                return null;
            }
        }
        public async Task<string?> UploadProofAsync(IFormFile file, string? orderId, string? customerId)
        {
            try
            {
                using var content = new MultipartFormDataContent();

                await using var stream = file.OpenReadStream();
                var fileContent = new StreamContent(stream);
                fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);

                content.Add(fileContent, "file", file.FileName);

                if (!string.IsNullOrEmpty(orderId))
                    content.Add(new StringContent(orderId), "orderId");
                if (!string.IsNullOrEmpty(customerId))
                    content.Add(new StringContent(customerId), "customerId");

                var response = await _client.PostAsync("proofs/upload", content);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<Dictionary<string, string?>>();
                if (result != null && result.TryGetValue("url", out var url))
                    return url;

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UploadProofAsync failed for file {FileName}", file.FileName);
                return null;
            }
        }

    }
}

namespace ABCRetailers.Models.FunctionsDtos
{
    public class HomeViewModelDto
    {
        public int CustomerCount { get; set; }
        public int ProductCount { get; set; }
        public int OrderCount { get; set; }

        public List<ProductDto> FeaturedProducts { get; set; } = new List<ProductDto>();
    }
    public class CustomerDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Surname { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string ShippingAddress { get; set; } = string.Empty;
    }

    public class CreateCustomerDto
    {
        public string Name { get; set; } = string.Empty;
        public string Surname { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string ShippingAddress { get; set; } = string.Empty;
    }

    public class UpdateCustomerDto : CreateCustomerDto { }

    public class ProductDto
    {
        public string Id { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public double Price { get; set; }
        public int StockAvailable { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
    }

    public class CreateProductDto
    {
        public string ProductName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public double Price { get; set; }
        public int StockAvailable { get; set; }
        public string? ImageUrl { get; set; }
    }

    public class UpdateProductDto : CreateProductDto
    {
        public string Id { get; set; } = string.Empty; 
    }


    public class OrderDto
    {
        public string Id { get; set; } = string.Empty;
        public string CustomerId { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string ProductId { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public double UnitPrice { get; set; }
        public int Quantity { get; set; }
        public double TotalPrice { get; set; }
        public string Status { get; set; } = string.Empty;
        public string OrderDate { get; set; } = string.Empty;
    }

    public class CreateOrderDto
    {
        public string CustomerId { get; set; } = string.Empty;
        public string ProductId { get; set; } = string.Empty;
        public int Quantity { get; set; }
    }

    public class UpdateOrderStatusDto
    {
        public string NewStatus { get; set; } = string.Empty;
    }
}

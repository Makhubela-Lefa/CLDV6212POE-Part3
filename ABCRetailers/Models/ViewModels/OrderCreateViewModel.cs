using System.ComponentModel.DataAnnotations;
using ABCRetailers.Models.FunctionsDtos;

namespace ABCRetailers.Models.ViewModels
{
    public class OrderCreateViewModel
    {
        [Required]
        [Display(Name = "Customer")]
        public string CustomerId { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Product")]
        public string ProductId { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Quantity")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; }

        [Required]
        [Display(Name = "Order Date")]
        [DataType(DataType.Date)]
        public DateTime OrderDate { get; set; } = DateTime.Today;

        [Required]
        [Display(Name = "Status")]
        public string Status { get; set; } = "Submitted";

        // These are for the dropdowns
        public List<CustomerDto> Customers { get; set; } = new();
        public List<ProductDto> Products { get; set; } = new();

        // --- ADD THESE PROPERTIES ---
        public string Id { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public string Username { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
    }
}
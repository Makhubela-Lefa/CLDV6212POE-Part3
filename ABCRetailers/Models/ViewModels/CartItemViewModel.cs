namespace ABCRetailers.Models.ViewModels
{
    public class CartItemViewModel
    {
        // The ID of the cart record
        public int Id { get; set; }

        // Username of the customer
        public string CustomerUsername { get; set; } = string.Empty;

        // The product linked to the cart
        public string ProductId { get; set; } = string.Empty;

        // The name of the product (from Product table or model)
        public string ProductName { get; set; } = string.Empty;

        // Quantity of that product in the cart
        public int Quantity { get; set; }

        // The price per item (from the Product table)
        public decimal UnitPrice { get; set; }

        // Total for this line item (calculated)
        public decimal Subtotal => Quantity * UnitPrice;
    }

    public class CartPageViewModel
    {
        public List<CartItemViewModel> Items { get; set; } = new();
        public decimal Total => Items.Sum(i => i.Subtotal);
    }
}

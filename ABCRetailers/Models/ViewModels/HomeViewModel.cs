//Models/ViewModels/HomeViewModel.cs
using ABCRetailers.Models.FunctionsDtos;
namespace ABCRetailers.Models.ViewModels
{
    public class HomeViewModel
    {
        public List<ProductDto> FeaturedProducts { get; set; } = new();
        public int CustomerCount { get; set; }
        public int ProductCount { get; set; }
        public int OrderCount { get; set; }
    }
}

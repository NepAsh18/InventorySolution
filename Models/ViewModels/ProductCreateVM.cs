using System.ComponentModel.DataAnnotations;

namespace InventorySolution.Models.ViewModels
{
    public class ProductCreateVM
    {
        [Required]
        public string Name { get; set; }
        public string Description { get; set; }
        [Required]
        public int CategoryId { get; set; }
        [Required]
        public int UnitMeasureId { get; set; }
        public int ReorderLevel { get; set; }
        public string ImagePath { get; set; }
        [Required]
        public decimal BasePrice { get; set; }

        [Required]
        public int Quantity { get; set; }
        [Required]
        public decimal PurchasePrice { get; set; }
        [Required]
        public DateTime ManufacturedDate { get; set; } = DateTime.Today;
        [Required]
        public DateTime ExpiryDate { get; set; } = DateTime.Today.AddYears(1);
    }
}
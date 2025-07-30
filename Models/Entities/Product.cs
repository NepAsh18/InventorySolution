using Microsoft.AspNetCore.Components.Routing;
using System.ComponentModel.DataAnnotations;

namespace InventorySolution.Models.Entities
{
    public class Product
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string Description { get; set; }
        [Required]
        public DateTime ManufacturedDate { get; set; }
        [Required]
        public DateTime ExpiryDate { get; set; }
        public string ImagePath { get; set; }

        [Required]
        public decimal Price { get; set; }

        [Required]
        public int Quantity { get; set; }

        public int ReorderLevel { get; set; }

        // Foreign Keys
        [Required]
        public int CategoryId { get; set; }

        public Category Category { get; set; }
        

        [Required]
        public int UnitMeasureId { get; set; }

        public UnitMeasure UnitMeasure { get; set; }

        public ICollection<OrderItem> OrderItems { get; set; }
    }

}


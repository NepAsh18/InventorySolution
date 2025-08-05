using System;
using System.ComponentModel.DataAnnotations;
using InventorySolution.Models.Entities;

namespace InventorySolution.Models.ViewModels
{
    public class BatchVM
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be positive")]
        public int Quantity { get; set; }
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be positive")]
        public decimal PurchasePrice { get; set; }
        [Required]
        public DateTime ManufacturedDate { get; set; } = DateTime.Today;
        [Required]
        public DateTime ExpiryDate { get; set; } = DateTime.Today.AddYears(1);
        public DateTime AddedDate { get; set; } = DateTime.UtcNow;
        public BatchStatus Status { get; set; }
    }
}
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventorySolution.Models.Entities
{
    public class ProductBatch
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; }
        public int Quantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PurchasePrice { get; set; }
        public DateTime ManufacturedDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public DateTime AddedDate { get; set; } = DateTime.UtcNow;

        // Status indicator
        public BatchStatus Status => ExpiryDate < DateTime.Today ? BatchStatus.Expired
            : (ExpiryDate < DateTime.Today.AddMonths(1) ? BatchStatus.ExpiringSoon
            : BatchStatus.Active);
    }

    public enum BatchStatus
    {
        Active,
        ExpiringSoon,
        Expired
    }
}
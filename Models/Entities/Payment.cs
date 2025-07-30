using System.ComponentModel.DataAnnotations.Schema;

namespace InventorySolution.Models.Entities
{
    public class Payment
    {
        public int Id { get; set; }
        public string Provider { get; set; } = "CashOnDelivery"; // Default
        public string TransactionId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; } = DateTime.Now;

        [ForeignKey("Order")]
        public int OrderId { get; set; }
        public virtual Order Order { get; set; }
    }
}
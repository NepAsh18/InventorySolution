using System.ComponentModel.DataAnnotations.Schema;

namespace InventorySolution.Models.Entities
{
    public class Order
    {
        public int Id { get; set; }

        [ForeignKey("User")]
        public string UserId { get; set; } = string.Empty;
        public virtual AppUser User { get; set; }

        [ForeignKey("Location")]
        public int LocationId { get; set; }
        public virtual Location Location { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.Now;
        public DateTime LastStatusChange { get; set; } = DateTime.Now;
        public decimal TotalAmount { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        public string PaymentMethod { get; set; } = "CashOnDelivery";

        // Navigation properties
        public virtual List<OrderItem> Items { get; set; } = new List<OrderItem>();
        public virtual Payment Payment { get; set; }
        public bool IsDirectOrder { get; set; } = false;
    }
}
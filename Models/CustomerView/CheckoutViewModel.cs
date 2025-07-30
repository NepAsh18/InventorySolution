using System.ComponentModel.DataAnnotations.Schema;

namespace InventorySolution.Models.CustomerView
{
    public class CheckoutViewModel
    {
        public Cart Cart { get; set; } = new Cart();
        public int LocationId { get; set; }
        public string PaymentMethod { get; set; } = "CashOnDelivery";

        [NotMapped]
        public double LocationFee { get; set; }

        [NotMapped]
        public double TotalWithLocationFee => (double)Cart.Total + LocationFee;
        public bool IsDirectCheckout { get; set; }
       
    }
}
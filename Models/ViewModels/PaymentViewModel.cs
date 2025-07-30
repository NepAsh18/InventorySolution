// Models/PaymentView/PaymentViewModel.cs
namespace InventorySolution.Models.ViewModels
{
    public class PaymentViewModel
    {
        public string Method { get; set; }
        public int OrderId { get; set; }
        public int AttemptsLeft { get; set; }
        public string Error { get; set; }
    }
}
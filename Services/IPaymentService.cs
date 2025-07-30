namespace InventorySolution.Services
{
    public interface IPaymentService
    {
        Task<PaymentResult> ProcessPaymentAsync(string method, decimal amount);
    }

    public class PaymentResult
    {
        public bool Success { get; set; }
        public string TransactionId { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
    }
}


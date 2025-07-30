using System;
using System.Threading.Tasks;

namespace InventorySolution.Services
{
    public class PaymentService : IPaymentService
    {
        public async Task<PaymentResult> ProcessPaymentAsync(string method, decimal amount)
        {
            // Simulate payment processing delay
            await Task.Delay(500);

            return method switch
            {
                "Cash" => new PaymentResult
                {
                    Success = true,
                    TransactionId = $"CASH-{Guid.NewGuid().ToString()[..8].ToUpper()}"
                },

                "Khalti" => new PaymentResult
                {
                    Success = true,
                    TransactionId = $"KH-{DateTime.Now:yyyyMMddHHmm}-{Guid.NewGuid().ToString()[..4].ToUpper()}"
                },

                "Esewa" => new PaymentResult
                {
                    Success = true,
                    TransactionId = $"ESEWA-{DateTime.Now:MMddHHmm}-{Guid.NewGuid().ToString()[..6].ToUpper()}"
                },

                _ => new PaymentResult
                {
                    Success = false,
                    ErrorMessage = $"Unsupported payment method: {method}"
                }
            };
        }
    }
}
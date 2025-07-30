// Controllers/PaymentController.cs
using InventorySolution.Data;
using InventorySolution.Models.Entities;
using InventorySolution.Models.ViewModels;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace InventorySolution.Controllers
{
    public class PaymentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(
            ApplicationDbContext context,
            ILogger<PaymentController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult InitiatePayment(string method, int orderId)
        {
            HttpContext.Session.SetInt32($"PaymentAttempts_{orderId}", 0);
            var viewModel = new PaymentViewModel
            {
                Method = method,
                OrderId = orderId,
                AttemptsLeft = 3
            };

            return View($"~/Views/Payments/{method}Login.cshtml", viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> VerifyCredentials(string method, int orderId, string username, string mpin)
        {
            int attemptCount = HttpContext.Session.GetInt32($"PaymentAttempts_{orderId}") ?? 0;
            bool isValid = ValidateCredentials(method, username, mpin);
            var viewModel = new PaymentViewModel
            {
                Method = method,
                OrderId = orderId,
                AttemptsLeft = 3 - attemptCount
            };

            if (!isValid)
            {
                attemptCount++;
                HttpContext.Session.SetInt32($"PaymentAttempts_{orderId}", attemptCount);
                viewModel.AttemptsLeft = 3 - attemptCount;
                viewModel.Error = "Invalid credentials. Please try again.";

                if (attemptCount >= 3)
                {
                    return await HandlePaymentFailure(orderId, "Maximum attempts exceeded. Payment failed.");
                }

                return View($"~/Views/Payments/{method}Login.cshtml", viewModel);
            }

            return View($"~/Views/Payments/{method}OTP.cshtml", viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> VerifyOTP(string method, int orderId, string otp)
        {
            int attemptCount = HttpContext.Session.GetInt32($"PaymentAttempts_{orderId}") ?? 0;
            bool isValidOTP = ValidateOTP(method, otp);
            var viewModel = new PaymentViewModel
            {
                Method = method,
                OrderId = orderId,
                AttemptsLeft = 3 - attemptCount
            };

            if (!isValidOTP)
            {
                attemptCount++;
                HttpContext.Session.SetInt32($"PaymentAttempts_{orderId}", attemptCount);
                viewModel.AttemptsLeft = 3 - attemptCount;
                viewModel.Error = "Invalid OTP. Please try again.";

                if (attemptCount >= 3)
                {
                    return await HandlePaymentFailure(orderId, "Invalid OTP. Maximum attempts exceeded.");
                }

                return View($"~/Views/Payments/{method}OTP.cshtml", viewModel);
            }

            return await CompletePayment(orderId, method);
        }

        [HttpPost]
        public async Task<IActionResult> CancelPayment(int orderId)
        {
            return await HandlePaymentFailure(orderId, "Payment was cancelled by user.");
        }

        private bool ValidateCredentials(string method, string username, string mpin)
        {
            // eSewa validation
            if (method.Equals("eSewa", StringComparison.OrdinalIgnoreCase))
            {
                string[] validIds = { "9806800001", "9806800002", "9806800003", "9806800004", "9806800005" };
                return Array.Exists(validIds, id => id == username) && mpin == "1122";
            }

            // Khalti validation
            if (method.Equals("Khalti", StringComparison.OrdinalIgnoreCase))
            {
                string[] validIds = {
                    "9800000000", "9800000001", "9800000002",
                    "9800000003", "9800000004", "9800000005"
                };
                return Array.Exists(validIds, id => id == username) && mpin == "1111";
            }

            return false;
        }

        private bool ValidateOTP(string method, string otp)
        {
            return method.Equals("eSewa", StringComparison.OrdinalIgnoreCase) ? otp == "123456"
                 : method.Equals("Khalti", StringComparison.OrdinalIgnoreCase) ? otp == "987654"
                 : false;
        }

        private async Task<IActionResult> CompletePayment(int orderId, string method)
        {
            var order = await _context.Orders
                .Include(o => o.Payment)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null || order.Status != OrderStatus.Pending)
            {
                return Json(new { success = false, error = "Order not found or already processed" });
            }

            // Create payment record
            order.Payment = new Payment
            {
                Provider = method,
                Amount = order.TotalAmount,
                TransactionId = $"{method}-{Guid.NewGuid().ToString()[..8]}",
                PaymentDate = DateTime.Now
            };

            // Update order status
            order.Status = OrderStatus.Processing;
            order.LastStatusChange = DateTime.Now;

            await _context.SaveChangesAsync();
            HttpContext.Session.Remove($"PaymentAttempts_{orderId}");

            return Json(new
            {
                success = true,
                redirectUrl = Url.Action("Confirmation", "Checkout", new { id = orderId })
            });
        }

        private async Task<IActionResult> HandlePaymentFailure(int orderId, string errorMessage)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order != null && order.Status == OrderStatus.Pending)
            {
                order.Status = OrderStatus.Cancelled;
                order.LastStatusChange = DateTime.Now;
                await _context.SaveChangesAsync();
            }

            HttpContext.Session.Remove($"PaymentAttempts_{orderId}");
            return Json(new
            {
                success = false,
                error = errorMessage,
                redirectUrl = Url.Action("Index", "Checkout")
            });
        }
    }
}
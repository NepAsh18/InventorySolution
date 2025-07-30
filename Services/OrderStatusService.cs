using InventorySolution.Data;
using InventorySolution.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace InventorySolution.Services
{
    public class OrderStatusService : IOrderStatusService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<OrderStatusService> _logger;

        public OrderStatusService(ApplicationDbContext context,
                                  ILogger<OrderStatusService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task UpdateOrderStatusesAsync()
        {
            var orders = await _context.Orders
                .Where(o => o.Status != OrderStatus.Delivered &&
                           o.Status != OrderStatus.Cancelled)
                .ToListAsync();

            foreach (var order in orders)
            {
                var timeInCurrentStatus = DateTime.Now - order.LastStatusChange;

                try
                {
                    if (IsDigitalPayment(order.PaymentMethod))
                    {
                        // Digital payments (Esewa/Khalti): Complete in 2 hours
                        await ProcessDigitalOrder(order, timeInCurrentStatus);
                    }
                    else
                    {
                        // Cash on delivery: Complete in 3 hours
                        await ProcessCashOrder(order, timeInCurrentStatus);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error updating status for order {order.Id}");
                }
            }
        }

        private bool IsDigitalPayment(string paymentMethod)
        {
            return paymentMethod == "Khalti" || paymentMethod == "Esewa";
        }

        private async Task ProcessDigitalOrder(Order order, TimeSpan timeInCurrentStatus)
        {
            switch (order.Status)
            {
                case OrderStatus.Paid when timeInCurrentStatus.TotalMinutes >= 30:
                    await UpdateOrderStatus(order, OrderStatus.Processing);
                    break;

                case OrderStatus.Processing when timeInCurrentStatus.TotalMinutes >= 60:
                    await UpdateOrderStatus(order, OrderStatus.Shipped);
                    break;

                case OrderStatus.Shipped when timeInCurrentStatus.TotalMinutes >= 120:
                    await UpdateOrderStatus(order, OrderStatus.Delivered);
                    break;
            }
        }

        private async Task ProcessCashOrder(Order order, TimeSpan timeInCurrentStatus)
        {
            switch (order.Status)
            {
                case OrderStatus.Pending when timeInCurrentStatus.TotalMinutes >= 60:
                    await UpdateOrderStatus(order, OrderStatus.Processing);
                    break;

                case OrderStatus.Processing when timeInCurrentStatus.TotalMinutes >= 120:
                    await UpdateOrderStatus(order, OrderStatus.Shipped);
                    break;

                case OrderStatus.Shipped when timeInCurrentStatus.TotalMinutes >= 180:
                    await UpdateOrderStatus(order, OrderStatus.Delivered);
                    break;
            }
        }

        private async Task UpdateOrderStatus(Order order, OrderStatus newStatus)
        {
            order.Status = newStatus;
            order.LastStatusChange = DateTime.Now;
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Order {order.Id} updated to {newStatus}");
        }
    }
}
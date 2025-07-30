// StatusController.cs
using InventorySolution.Data;
using InventorySolution.Models.Entities;
using InventorySolution.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InventorySolution.Controllers
{
    [Authorize(Roles = "Admin")]
    public class StatusController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly INotificationService _notificationService;

        public StatusController(
            ApplicationDbContext context,
            UserManager<AppUser> userManager,
            INotificationService notificationService)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        }

        [HttpGet]
        public async Task<IActionResult> Index(int? selectedOrderId = null)
        {
            try
            {
                // Get current month range in UTC
                var now = DateTime.UtcNow;
                var firstDayOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddTicks(-1);

                ViewBag.PendingOrderCount = await _context.Orders
                    .CountAsync(o => o.Status == OrderStatus.Pending &&
                                   o.OrderDate >= firstDayOfMonth &&
                                   o.OrderDate <= lastDayOfMonth);

                var orders = await _context.Orders
                    .AsNoTracking()
                    .Where(o => o.OrderDate >= firstDayOfMonth && o.OrderDate <= lastDayOfMonth)
                    .Include(o => o.User)
                    .Include(o => o.Location)
                    .Include(o => o.Payment)
                    .OrderByDescending(o => o.OrderDate)
                    .ToListAsync();

                ViewBag.SelectedOrderId = selectedOrderId;
                return View(orders);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error loading orders";
                return View(new List<Order>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> OrderDetails(int id)
        {
            if (id <= 0) return BadRequest("Invalid order ID");

            try
            {
                var order = await _context.Orders
                    .AsNoTracking()
                    .Include(o => o.User)
                    .Include(o => o.Location)
                    .Include(o => o.Items)
                        .ThenInclude(oi => oi.Product)
                    .Include(o => o.Payment)
                    .FirstOrDefaultAsync(o => o.Id == id);

                return order == null
                    ? NotFound()
                    : PartialView("_OrderDetailsPartial", order);
            }
            catch (Exception)
            {
                return StatusCode(500, "Error retrieving order details");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, OrderStatus status)
        {
            if (id <= 0 || !Enum.IsDefined(typeof(OrderStatus), status))
                return BadRequest("Invalid order ID or status");

            try
            {
                var order = await _context.Orders
                    .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                    .FirstOrDefaultAsync(o => o.Id == id);

                if (order == null) return NotFound();

                // Prevent modifying terminal states
                if (order.Status == OrderStatus.Cancelled || order.Status == OrderStatus.Delivered)
                {
                    TempData["Error"] = $"Cannot modify {order.Status} orders";
                    return RedirectToAction("Index", new { selectedOrderId = id });
                }

                var previousStatus = order.Status;
                if (previousStatus == status)
                {
                    TempData["Info"] = "Status is unchanged";
                    return RedirectToAction("Index", new { selectedOrderId = id });
                }

                // Handle status-specific actions
                if (status == OrderStatus.Cancelled)
                {
                    // Restore inventory
                    foreach (var item in order.Items)
                    {
                        var product = await _context.Products.FindAsync(item.ProductId);
                        if (product != null) product.Quantity += item.Quantity;
                    }
                }

                // Update order status
                order.Status = status;
                order.LastStatusChange = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                // Send notifications for important status changes
                if (status == OrderStatus.Delivered)
                {
                    await _notificationService.CreateNotificationAsync(
                        $"Order #{order.Id} completed",
                        "OrderCompleted",
                        order.Id
                    );
                }
                else if (status == OrderStatus.Cancelled)
                {
                    await _notificationService.CreateNotificationAsync(
                        $"Order #{order.Id} cancelled by admin",
                        "OrderCancelled",
                        order.Id
                    );
                }

                // Optional: Notify about all status changes
                await _notificationService.CreateNotificationAsync(
                    $"Order #{order.Id} status changed: {previousStatus} → {status}",
                    "OrderStatusChanged",
                    order.Id
                );

                TempData["Success"] = $"Status updated to {status}";
                return RedirectToAction("Index", new { selectedOrderId = id });
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error updating status";
                return RedirectToAction("Index", new { selectedOrderId = id });
            }
        }
    }
}
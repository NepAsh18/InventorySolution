using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using InventorySolution.Data;
using InventorySolution.Models.CustomerView;
using InventorySolution.Models.Entities;
using InventorySolution.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace InventorySolution.Controllers;

[Authorize]
public class CheckoutController(
    ApplicationDbContext context,
    LocationPaymentService locationPaymentService,
    INotificationService notificationService,
    ILogger<CheckoutController> logger) : Controller
{
    public IActionResult Index()
    {
        var directCart = HttpContext.Session.Get<Cart>("DirectCart");
        if (directCart != null && directCart.Items.Count > 0)
        {
            ViewBag.IsDirectCheckout = true;
            ViewBag.Locations = GetLocations();
            return View(new CheckoutViewModel { Cart = directCart });
        }

        var cart = HttpContext.Session.Get<Cart>("Cart") ?? new Cart();
        if (cart.Items.Count == 0) return RedirectToAction("Index", "Cart");

        ViewBag.IsDirectCheckout = false;
        ViewBag.Locations = GetLocations();
        return View(new CheckoutViewModel { Cart = cart });
    }

    [HttpGet]
    public async Task<IActionResult> GetLocationFee(int locationId)
    {
        var fee = await locationPaymentService.GetLocationPrice(locationId);
        return Json(new { fee });
    }

    [HttpPost]
    public async Task<IActionResult> PlaceOrder(CheckoutViewModel model)
    {
        logger.LogInformation("PlaceOrder action started");

        Cart cart = model.IsDirectCheckout ? model.Cart :
                   HttpContext.Session.Get<Cart>("Cart") ?? new Cart();

        if (cart.Items.Count == 0)
        {
            logger.LogWarning("PlaceOrder called with empty cart");
            return RedirectToAction("Index", "Cart");
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        double locationFee = await locationPaymentService.GetLocationPrice(model.LocationId);
        double totalAmount = (double)cart.Total + locationFee;

        var order = new Order
        {
            UserId = userId,
            LocationId = model.LocationId,
            TotalAmount = (decimal)totalAmount,
            PaymentMethod = model.PaymentMethod,
            LastStatusChange = DateTime.Now,
            Status = OrderStatus.Pending
        };

        using var transaction = await context.Database.BeginTransactionAsync();
        try
        {
            foreach (var item in cart.Items)
            {
                var product = await context.Products.FindAsync(item.ProductId);
                if (product == null)
                {
                    logger.LogWarning("Product {ProductId} not found", item.ProductId);
                    continue;
                }

                if (product.Quantity < item.Quantity)
                {
                    string error = $"{product.Name} is out of stock. Only {product.Quantity} available";
                    logger.LogWarning(error);
                    TempData["Error"] = error;
                    return RedirectToAction("Index");
                }

                // Update inventory immediately
                product.Quantity -= item.Quantity;

                order.Items.Add(new OrderItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = item.BasePrice,
                    FinalPrice = item.FinalPrice
                });
            }

            context.Orders.Add(order);
            await context.SaveChangesAsync();
            logger.LogInformation("Order created: {OrderId}", order.Id);

            if (model.PaymentMethod == "Cash")
            {
                logger.LogInformation("Processing cash payment for order {OrderId}", order.Id);
                order.Payment = new Payment
                {
                    Provider = "Cash",
                    Amount = (decimal)totalAmount,
                    TransactionId = "CASH-" + Guid.NewGuid().ToString()[..8],
                    PaymentDate = DateTime.Now
                };
                order.Status = OrderStatus.Processing;
                await context.SaveChangesAsync();
            }

            await transaction.CommitAsync();
            await notificationService.CreateNotificationAsync(
    $"New order placed: #{order.Id}",
    "OrderPlaced",
    order.Id
);

            // Clear cart sessions only after successful transaction
            if (model.IsDirectCheckout)
            {
                HttpContext.Session.Remove("DirectCart");
                logger.LogInformation("DirectCart session cleared");
            }
            else
            {
                HttpContext.Session.Remove("Cart");
                logger.LogInformation("Cart session cleared");
            }

            if (model.PaymentMethod == "Cash")
            {
                return RedirectToAction("Confirmation", new { id = order.Id });
            }

            // Validate payment method for online payments
            if (model.PaymentMethod != "eSewa" && model.PaymentMethod != "Khalti")
            {
                logger.LogError("Invalid payment method: {PaymentMethod}", model.PaymentMethod);
                TempData["Error"] = "Invalid payment method selected";
                return RedirectToAction("Index");
            }

            return Json(new
            {
                requiresPayment = true,
                orderId = order.Id,
                paymentMethod = model.PaymentMethod
            });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            logger.LogError(ex, "Error placing order");
            TempData["Error"] = "Error processing your order. Please try again.";
            return RedirectToAction("Index");
        }
    }

    public IActionResult Confirmation(int id)
    {
        logger.LogInformation("Showing confirmation for order {OrderId}", id);
        var order = context.Orders
            .Include(o => o.Location)
            .Include(o => o.Payment)
            .Include(o => o.Items)
                .ThenInclude(i => i.Product)
            .FirstOrDefault(o => o.Id == id);

        if (order == null)
        {
            logger.LogWarning("Order {OrderId} not found for confirmation", id);
            return NotFound();
        }

        // Ensure payment details exist
        if (order.Payment == null && order.PaymentMethod == "Cash")
        {
            logger.LogWarning("Payment record missing for cash order {OrderId}", id);
            order.Payment = new Payment
            {
                Provider = "Cash",
                Amount = order.TotalAmount,
                TransactionId = "N/A",
                PaymentDate = DateTime.Now
            };
        }

        return View(order);
    }

    [HttpPost]
    public async Task<IActionResult> CancelOrder(int id)
    {
        logger.LogInformation("Canceling order {OrderId}", id);
        var order = await context.Orders
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
        {
            logger.LogWarning("Order {OrderId} not found for cancellation", id);
            return NotFound();
        }

        if (order.Status == OrderStatus.Delivered || order.Status == OrderStatus.Cancelled)
        {
            string error = "Order cannot be cancelled at this stage";
            logger.LogWarning(error);
            TempData["Error"] = error;
            return RedirectToAction("Details", new { id });
        }

        using var transaction = await context.Database.BeginTransactionAsync();
        try
        {
            foreach (var item in order.Items)
            {
                var product = await context.Products.FindAsync(item.ProductId);
                if (product != null) product.Quantity += item.Quantity;
            }

            order.Status = OrderStatus.Cancelled;
            order.LastStatusChange = DateTime.Now;
            await context.SaveChangesAsync();
            await transaction.CommitAsync();

            await notificationService.CreateNotificationAsync(
    $"Order #{order.Id} has been cancelled",
    "OrderCancelled",    // ✅ Correct type
    order.Id             // ✅ Related ID
);

            TempData["Success"] = "Order cancelled successfully";
            logger.LogInformation("Order {OrderId} cancelled successfully", id);
            return RedirectToAction("Details", new { id });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            logger.LogError(ex, "Error cancelling order {OrderId}", id);
            TempData["Error"] = "Error cancelling order";
            return RedirectToAction("Details", new { id });
        }
    }

    public async Task<IActionResult> Details(int id)
    {
        logger.LogInformation("Showing details for order {OrderId}", id);
        var order = await context.Orders
            .Include(o => o.Items)
                .ThenInclude(i => i.Product)
            .Include(o => o.Location)
            .Include(o => o.Payment)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
        {
            logger.LogWarning("Order {OrderId} not found", id);
            return NotFound();
        }

        // Handle missing payment records
        if (order.Payment == null && order.PaymentMethod == "Cash")
        {
            order.Payment = new Payment
            {
                Provider = "Cash",
                Amount = order.TotalAmount,
                TransactionId = "N/A",
                PaymentDate = order.LastStatusChange
            };
        }

        return View(order);
    }

    public IActionResult OrderDetails(int id)
    {
        logger.LogInformation("Showing order details for {OrderId}", id);
        var order = context.Orders
            .Include(o => o.Payment)
            .Include(o => o.Location)
            .Include(o => o.Items)
                .ThenInclude(oi => oi.Product)
            .FirstOrDefault(o => o.Id == id);

        if (order == null)
        {
            logger.LogWarning("Order {OrderId} not found for details view", id);
            return NotFound();
        }

        // Handle missing payment records
        if (order.Payment == null && order.PaymentMethod == "Cash")
        {
            order.Payment = new Payment
            {
                Provider = "Cash",
                Amount = order.TotalAmount,
                TransactionId = "N/A",
                PaymentDate = order.LastStatusChange
            };
        }

        return View("~/Views/CustomerProduct/OrderDetails.cshtml", order);
    }

    private SelectList GetLocations()
    {
        return new SelectList(context.Locations.ToList(), "Id", "Name");
    }
}
using InventorySolution.Data;
using InventorySolution.Models.CustomerView;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

public class OrderNowController(ApplicationDbContext context) : Controller
{
    [HttpPost]
    public IActionResult OrderNow(int productId, int quantity)
    {
        var product = context.Products.Find(productId);
        if (product == null || quantity < 1) return NotFound();

        // Create temporary direct cart
        var directCart = new Cart
        {
            Items = [new CartItem
            {
                ProductId = product.Id,
                Name = product.Name,
                Quantity = quantity,
                BasePrice = product.Price,
                FinalPrice = product.Price,
                ImagePath = product.ImagePath
            }],
            Total = product.Price * quantity
        };

        // Store in session while preserving regular cart
        HttpContext.Session.Set("DirectCart", directCart);

        // Clear any existing regular cart to prevent conflicts
        HttpContext.Session.Remove("Cart");

        return User.Identity?.IsAuthenticated == true
            ? RedirectToAction("Index", "Checkout")
            : RedirectToAction("Login", "Account", new { returnUrl = Url.Action("Index", "Checkout") });
    }
}
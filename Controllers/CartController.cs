using Inventory.Services;
using InventorySolution.Data;
using InventorySolution.Models.CustomerView;
using InventorySolution.Models.Entities;
using InventorySolution.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace InventorySolution.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly PricingService _pricingService;
        public CartController(ApplicationDbContext context, PricingService pricingService)
        {
            _context = context;
            _pricingService = pricingService;
        }

        public IActionResult Index()
        {
            var cart = HttpContext.Session.Get<Cart>("Cart") ?? new Cart();
            return View(cart);
        }

        [Authorize]
        [HttpPost]
        public IActionResult Checkout()
        {
            var cart = HttpContext.Session.Get<Cart>("Cart");
            if (cart == null || cart.Items.Count == 0)
            {
                TempData["Error"] = "Your cart is empty";
                return RedirectToAction("Index");
            }

            return RedirectToAction("Index", "Checkout");
        }

        [HttpPost]
        public IActionResult UpdateCart(int productId, int quantity)
        {
            var cart = HttpContext.Session.Get<Cart>("Cart") ?? new Cart();
            var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);

            if (item != null)
            {
                if (quantity <= 0)
                {
                    cart.Items.Remove(item);
                }
                else
                {
                    item.Quantity = quantity;
                }
            }

            // Recalculate total
            cart.Total = cart.Items.Sum(i => i.FinalPrice * i.Quantity);

            HttpContext.Session.Set("Cart", cart);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult RemoveFromCart(int productId)
        {
            var cart = HttpContext.Session.Get<Cart>("Cart") ?? new Cart();
            var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);

            if (item != null)
            {
                cart.Items.Remove(item);
                cart.Total = cart.Items.Sum(i => i.FinalPrice * i.Quantity);
                HttpContext.Session.Set("Cart", cart);
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult AddToCart(int productId, int quantity)
        {
            var product = _context.Products.Find(productId);
            if (product == null)
            {
                return NotFound();
            }

            var cart = HttpContext.Session.Get<Cart>("Cart") ?? new Cart();
            var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == productId);

            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
            }
            else
            {
                cart.Items.Add(new CartItem
                {
                    ProductId = product.Id,
                    Name = product.Name,
                    BasePrice = product.Price,
                    FinalPrice = product.Price,
                    Quantity = quantity,
                    ImagePath = product.ImagePath
                });
            }

            cart.Total = cart.Items.Sum(i => i.FinalPrice * i.Quantity);
            HttpContext.Session.Set("Cart", cart);

            TempData["CartMessage"] = $"{product.Name} added to cart!";
            return RedirectToAction("Details", "CustomerProduct", new { id = productId });
        }
        [HttpPost]
        public IActionResult OrderNow(int productId, int quantity)
        {
            var product = _context.Products.Find(productId);
            if (product == null) return NotFound();

            // Get or create cart
            var cart = HttpContext.Session.Get<Cart>("Cart") ?? new Cart { Items = new List<CartItem>() };

            // Clear existing items (to simulate direct checkout)
            cart.Items.Clear();

            // Add current product as the only item
            cart.Items.Add(new CartItem
            {
                ProductId = product.Id,
                Name = product.Name,
                BasePrice = product.Price,
                FinalPrice = _pricingService.CalculateDynamicPrice(quantity, product.Price),
                Quantity = quantity,
                ImagePath = product.ImagePath
            });

            cart.Total = cart.Items.Sum(i => i.FinalPrice * i.Quantity);

            // Update session
            HttpContext.Session.Set("Cart", cart);

            // Redirect based on authentication
            return User.Identity?.IsAuthenticated == true
                ? RedirectToAction("Index", "Checkout")
                : RedirectToAction("Login", "Account", new { returnUrl = Url.Action("Index", "Checkout") });
        }
    }
}
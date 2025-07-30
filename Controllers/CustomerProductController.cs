using Inventory.Services;
using InventorySolution.Data;
using InventorySolution.Models.CustomerView;
using InventorySolution.Models.Entities;
using InventorySolution.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using X.PagedList;

namespace InventorySolution.Controllers
{
    public class CustomerProductController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ISearchService _searchService;

        public CustomerProductController(ApplicationDbContext context, ISearchService searchService)
        {
            _context = context;
            _searchService = searchService;
        }

        // Combined Index method with search and pagination
        public async Task<IActionResult> Index(string searchString, int? page)
        {
            ViewBag.SearchString = searchString;
            int pageSize = 20;
            int pageNumber = page ?? 1;

            // Get today's date once
            var today = DateTime.Today;

            IQueryable<Product> query = _context.Products
                .Include(p => p.Category)
                .Include(p => p.UnitMeasure)
                .Where(p => p.Quantity > 0)
                // Filter out expired products
                .Where(p => p.ExpiryDate == null || p.ExpiryDate >= today);

            // Apply search filter if searchString is provided
            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(p => p.Name.Contains(searchString));
            }

            // Get total count for efficient paging
            var totalCount = await query.CountAsync();

            // Get paginated data
            var products = await query
                .OrderBy(p => p.Name)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Create the paged list
            var pagedList = new StaticPagedList<Product>(
                products,
                pageNumber,
                pageSize,
                totalCount);

            return View(pagedList);
        }


        [Authorize(Roles = "User")]
        [HttpPost]
        public IActionResult AddToCart(int productId, int quantity, [FromServices] PricingService pricingService)
        {
            var product = _context.Products.Find(productId);
            if (product == null) return NotFound();

            // Get user ID
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Get or create cart
            var cart = HttpContext.Session.Get<Cart>("Cart") ?? new Cart();

            // Calculate dynamic price
            decimal finalPrice = pricingService.CalculateDynamicPrice(quantity, product.Price);

            // Add to cart
            var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == productId);
            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
                existingItem.FinalPrice = pricingService.CalculateDynamicPrice(existingItem.Quantity, product.Price);
            }
            else
            {
                cart.Items.Add(new CartItem
                {
                    ProductId = product.Id,
                    Name = product.Name,
                    ImagePath = product.ImagePath,
                    Quantity = quantity,
                    BasePrice = product.Price,
                    FinalPrice = finalPrice
                });
            }

            // Update cart total
            cart.Total = cart.Items.Sum(i => i.FinalPrice * i.Quantity);

            // Save cart to session
            HttpContext.Session.Set("Cart", cart);

            TempData["CartMessage"] = $"{product.Name} added to cart!";
            return RedirectToAction("Details", new { id = productId });
        }

        [Authorize(Roles = "User")]
        public async Task<IActionResult> Details(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.UnitMeasure)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null) return NotFound();

            // Record recently viewed
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await RecordRecentlyViewed(userId, product.Id);

            // Get recently viewed products (excluding current)
            var recentProducts = await GetRecentlyViewedProducts(userId, id);

            ViewBag.RecentProducts = recentProducts;
            return View(product);
        }

        private async Task RecordRecentlyViewed(string userId, int productId)
        {
            // Check if already exists
            var existing = await _context.RecentlyViewed
                .FirstOrDefaultAsync(r => r.UserId == userId && r.ProductId == productId);

            if (existing == null)
            {
                // Add new recently viewed
                _context.RecentlyViewed.Add(new RecentlyViewed
                {
                    UserId = userId,
                    ProductId = productId,
                    ViewedOn = DateTime.Now
                });
            }
            else
            {
                // Update timestamp
                existing.ViewedOn = DateTime.Now;
            }

            await _context.SaveChangesAsync();

            // Keep only last 5 viewed items
            var userViews = await _context.RecentlyViewed
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.ViewedOn)
                .Skip(5)
                .ToListAsync();

            if (userViews.Any())
            {
                _context.RecentlyViewed.RemoveRange(userViews);
                await _context.SaveChangesAsync();
            }
        }

        private async Task<List<Product>> GetRecentlyViewedProducts(string userId, int currentProductId)
        {
            var recentProductIds = await _context.RecentlyViewed
                .Where(r => r.UserId == userId && r.ProductId != currentProductId)
                .OrderByDescending(r => r.ViewedOn)
                .Take(5)
                .Select(r => r.ProductId)
                .ToListAsync();

            return await _context.Products
                .Include(p => p.Category)
                .Where(p => recentProductIds.Contains(p.Id))
                .ToListAsync();
        }

        [Authorize]
        [HttpPost]
        public IActionResult OrderNow(int productId, int quantity, [FromServices] PricingService pricingService)
        {
            var product = _context.Products.Find(productId);
            if (product == null) return NotFound();

            // Create a new cart with just this item
            var cart = new Cart();

            // Calculate dynamic price
            decimal finalPrice = pricingService.CalculateDynamicPrice(quantity, product.Price);

            cart.Items.Add(new CartItem
            {
                ProductId = product.Id,
                Name = product.Name,
                ImagePath = product.ImagePath,
                Quantity = quantity,
                BasePrice = product.Price,
                FinalPrice = finalPrice
            });

            cart.Total = cart.Items.Sum(i => i.FinalPrice * i.Quantity);

            // Save cart to session
            HttpContext.Session.Set("Cart", cart);

            return RedirectToAction("Index", "Checkout");
        }

        [Authorize(Roles = "User")]
        public async Task<IActionResult> OrderHistory()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var orders = await _context.Orders
                .Where(o => o.UserId == userId)
                .Include(o => o.Location)
                .Include(o => o.Items)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }
    }
}
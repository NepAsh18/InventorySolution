using InventorySolution.Data;
using InventorySolution.Models.Entities;
using InventorySolution.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InventorySolution.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ShipmentController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ShipmentController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // Calculate current month range
            var now = DateTime.Now;
            var firstDayOfMonth = new DateTime(now.Year, now.Month, 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

            // Get current month orders with related data
            var orders = await _context.Orders
                .Where(o => o.OrderDate >= firstDayOfMonth && o.OrderDate <= lastDayOfMonth)
                .Include(o => o.User)
                .Include(o => o.Location)
                .Include(o => o.Items)
                .ThenInclude(oi => oi.Product)
                .Include(o => o.Payment)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            // Predefined warehouses
            var warehouses = new List<string> { "Birgunj", "Nepalgunj", "Bhairawa", "Kathmandu", "Bharatpur" };
            var random = new Random();

            // Map to view models with random warehouses
            var viewModel = orders.Select(o => new ShipmentViewModel
            {
                OrderId = o.Id,
                CustomerName = o.User?.FullName ?? "Unknown Customer",
                CustomerLocation = o.Location?.Address ?? "Unknown Location",
                Status = o.Status,
                OrderDate = o.OrderDate,
                Warehouse = warehouses[random.Next(warehouses.Count)],
                TotalAmount = o.TotalAmount,
                LastStatusChange = o.LastStatusChange,
                ProgressPercentage = GetProgressPercentage(o.Status)
            }).ToList();

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.Location)
                .Include(o => o.Items)
                .ThenInclude(oi => oi.Product)
                .Include(o => o.Payment)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            // Predefined warehouses
            var warehouses = new List<string> { "Birgunj", "Nepalgunj", "Bhairawa", "Kathmandu", "Bharatpur" };
            var random = new Random();
            var warehouse = warehouses[random.Next(warehouses.Count)];

            var viewModel = new ShipmentDetailsViewModel
            {
                OrderId = order.Id,
                CustomerName = order.User?.FullName ?? "Unknown Customer",
                CustomerLocation = order.Location?.Address ?? "Unknown Location",
                Status = order.Status,
                OrderDate = order.OrderDate,
                Warehouse = warehouse,
                TotalAmount = order.TotalAmount,
                Items = order.Items.Select(oi => new ShipmentItemViewModel
                {
                    ProductName = oi.Product?.Name ?? "Unknown Product",
                    Quantity = oi.Quantity,
                    Price = oi.Product?.Price ?? 0
                }).ToList(),
                LastStatusChange = order.LastStatusChange,
                EstimatedDelivery = order.OrderDate.AddDays(7),
                ProgressPercentage = GetProgressPercentage(order.Status),
                RoutePoints = GenerateRoutePoints(warehouse, order.Location?.Address ?? "Unknown Location")
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            // Convert string to OrderStatus enum
            if (Enum.TryParse(status, out OrderStatus newStatus))
            {
                order.Status = newStatus;
                order.LastStatusChange = DateTime.Now;

                _context.Update(order);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Details", new { id });
        }

        private static int GetProgressPercentage(OrderStatus status)
        {
            return status switch
            {
                OrderStatus.Pending => 10,
                OrderStatus.Processing => 30,
                OrderStatus.Shipped => 50,
                OrderStatus.Delivered => 100,
                _ => 0
            };
        }

        private static List<string> GenerateRoutePoints(string warehouse, string destination)
        {
            var points = new List<string>();
            var random = new Random();
            var intermediatePoints = new List<string> { "Distribution Center", "Sorting Facility", "Transit Hub", "Local Depot" };

            points.Add(warehouse);

            // Add 1-3 intermediate points
            for (int i = 0; i < random.Next(1, 4); i++)
            {
                points.Add(intermediatePoints[random.Next(intermediatePoints.Count)]);
            }

            points.Add(destination);

            return points;
        }
    }
}
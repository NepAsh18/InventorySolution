#nullable enable

using Inventory.Data;
using Inventory.Utility;
using InventorySolution.Data;
using InventorySolution.Models.Entities;
using InventorySolution.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InventorySolution.Controllers
{
    public class AdminProductController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly StockAlertService _alertService;
        private readonly ISearchService _searchService;

        public AdminProductController(
            ApplicationDbContext context,
            StockAlertService alertService,
            ISearchService searchService)
        {
            _context = context;
            _alertService = alertService;
            _searchService = searchService;
        }

        // GET: AdminProduct
        public async Task<IActionResult> Index(string searchString, string sortOrder, string sortAlgorithm)
        {
            ViewBag.SearchString = searchString;
            var products = (await _searchService.SearchProductsByNameAsync(searchString)).ToList();

            // Set default sorting if not specified
            sortOrder ??= "date_desc";
            sortAlgorithm ??= "QuickSort";

            // Apply sorting
            if (sortAlgorithm == "MergeSort")
            {
                MergeSorter.Sort(products, GetSortField(sortOrder), IsAscending(sortOrder));
            }
            else
            {
                QuickSorter.Sort(products, GetSortField(sortOrder), IsAscending(sortOrder));
            }

            ViewBag.CriticalAlerts = _alertService.GetCriticalAlerts(products);
            ViewBag.SortOrder = sortOrder;
            ViewBag.SortAlgorithm = sortAlgorithm;

            return View(products);
        }

        private string GetSortField(string sortOrder)
        {
            return sortOrder switch
            {
                "name" or "name_desc" => "Name",
                "Date" or "date_desc" => "ManufacturedDate",
                "Id" or "id_desc" => "Id",
                _ => "ManufacturedDate"
            };
        }

        private bool IsAscending(string sortOrder)
        {
            return sortOrder switch
            {
                "name" or "Date" or "Id" => true,
                _ => false
            };
        }

        // GET: AdminProduct/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.UnitMeasure)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (product == null) return NotFound();

            return View(product);
        }

        // GET: AdminProduct/Create
        public IActionResult Create()
        {
            PopulateDropdowns();
            return View();
        }

        // POST: AdminProduct/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product)
        {
            // Validation: Expiry date after manufacture date
            if (product.ExpiryDate <= product.ManufacturedDate)
            {
                ModelState.AddModelError("ExpiryDate", "Expiry date must be after manufacture date");
            }

            // Validation: Quantity not negative
            if (product.Quantity < 0)
            {
                ModelState.AddModelError("Quantity", "Quantity cannot be negative");
            }

            if (ModelState.IsValid)
            {
                _context.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            PopulateDropdowns();
            return View(product);
        }

        // GET: AdminProduct/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.UnitMeasure)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (product == null) return NotFound();

            // Store existing image path in ViewData
            ViewData["ExistingImagePath"] = product.ImagePath;
            PopulateDropdowns();

            return View(product);
        }

        // POST: AdminProduct/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product product)
        {
            if (id != product.Id) return NotFound();

            // Retrieve existing image path
            var existingProduct = await _context.Products.AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);

            if (existingProduct == null)
            {
                return NotFound();
            }

            // Preserve existing image if new one isn't provided
            if (string.IsNullOrEmpty(product.ImagePath))
            {
                product.ImagePath = existingProduct.ImagePath;
            }

            // Validation: Expiry date after manufacture date
            if (product.ExpiryDate <= product.ManufacturedDate)
            {
                ModelState.AddModelError("ExpiryDate", "Expiry date must be after manufacture date");
            }

            // Validation: Quantity not negative
            if (product.Quantity < 0)
            {
                ModelState.AddModelError("Quantity", "Quantity cannot be negative");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(product);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.Id)) return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }

            // Preserve image path for re-display
            ViewData["ExistingImagePath"] = existingProduct.ImagePath;
            PopulateDropdowns();

            return View(product);
        }

        // GET: AdminProduct/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.UnitMeasure)
                .FirstOrDefaultAsync(m => m.Id == id);  // Fixed variable name here

            if (product == null) return NotFound();

            return View(product);
        }

        // POST: AdminProduct/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private void PopulateDropdowns()
        {
            ViewBag.Categories = _context.Categories.ToList();
            ViewBag.UnitMeasures = _context.UnitMeasures.ToList();
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }
    }
}
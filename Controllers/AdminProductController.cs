using Inventory.Data;
using InventorySolution.Models.Entities;
using Inventory.Utility;
using InventorySolution.Data;
using InventorySolution.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using InventorySolution.Models.ViewModels;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using Microsoft.AspNetCore.Http;

namespace InventorySolution.Controllers
{
    public class AdminProductController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly StockAlertService _alertService;
        private readonly ISearchService _searchService;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public AdminProductController(
            ApplicationDbContext context,
            StockAlertService alertService,
            ISearchService searchService,
            IWebHostEnvironment hostingEnvironment)
        {
            _context = context;
            _alertService = alertService;
            _searchService = searchService;
            _hostingEnvironment = hostingEnvironment;
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

        // GET: AdminProduct/Create
        public IActionResult Create()
        {
            PopulateDropdowns();
            return View(new ProductCreateVM());
        }

        // POST: AdminProduct/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductCreateVM model, IFormFile imageFile)
        {
            if (model.ExpiryDate <= model.ManufacturedDate)
            {
                ModelState.AddModelError("ExpiryDate", "Expiry date must be after manufacture date");
            }

            // Handle image upload
            if (imageFile != null && imageFile.Length > 0)
            {
                model.ImagePath = await SaveImageFile(imageFile);
            }

            if (ModelState.IsValid)
            {
                var product = new Product
                {
                    Name = model.Name,
                    Description = model.Description,
                    CategoryId = model.CategoryId,
                    UnitMeasureId = model.UnitMeasureId,
                    ReorderLevel = model.ReorderLevel,
                    ImagePath = model.ImagePath,
                    Price = model.BasePrice,
                    Quantity = model.Quantity,
                    ManufacturedDate = model.ManufacturedDate,
                    ExpiryDate = model.ExpiryDate
                };

                // Add initial batch
                product.Batches.Add(new ProductBatch
                {
                    Quantity = model.Quantity,
                    PurchasePrice = model.PurchasePrice,
                    ManufacturedDate = model.ManufacturedDate,
                    ExpiryDate = model.ExpiryDate
                });

                _context.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            PopulateDropdowns();
            return View(model);
        }

        // GET: AdminProduct/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.UnitMeasure)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null) return NotFound();

            var model = new ProductEditVM
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                CategoryId = product.CategoryId,
                UnitMeasureId = product.UnitMeasureId,
                ReorderLevel = product.ReorderLevel,
                ImagePath = product.ImagePath,
                BasePrice = product.Price
            };

            ViewData["ExistingImagePath"] = product.ImagePath;
            PopulateDropdowns();

            return View(model);
        }

       

        public async Task<IActionResult> ManageBatches(int? id)
        {
            if (id == null || id <= 0)
            {
                return RedirectToAction("Index");
            }

            var product = await _context.Products
                .Include(p => p.Batches)
                .Include(p => p.Category)
                .Include(p => p.UnitMeasure)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                TempData["BatchError"] = "Product not found";
                return RedirectToAction("Index");
            }

            var model = new ProductBatchesVM
            {
                Id = product.Id,
                Name = product.Name,
                CurrentQuantity = product.Quantity,
                ReorderLevel = product.ReorderLevel,
                Batches = product.Batches.OrderBy(b => b.AddedDate).Select(b => new BatchVM
                {
                    Id = b.Id,
                    ProductId = b.ProductId,
                    Quantity = b.Quantity,
                    PurchasePrice = b.PurchasePrice,
                    ManufacturedDate = b.ManufacturedDate,
                    ExpiryDate = b.ExpiryDate,
                    AddedDate = b.AddedDate,
                    Status = b.Status
                }).ToList(),
                NewBatch = new BatchVM
                {
                    ProductId = product.Id,
                    ManufacturedDate = DateTime.Today,
                    ExpiryDate = DateTime.Today.AddYears(1)
                }
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddBatch([Bind(Prefix = "NewBatch")] BatchVM model)
        {
            if (model.ProductId <= 0)
            {
                TempData["BatchError"] = "Invalid product ID";
                return RedirectToAction("Index");
            }

            if (!ModelState.IsValid)
            {
                TempData["BatchError"] = "Please correct the batch errors";
                return RedirectToAction("ManageBatches", new { id = model.ProductId });
            }

            var product = await _context.Products
                .Include(p => p.Batches)
                .FirstOrDefaultAsync(p => p.Id == model.ProductId);

            if (product == null)
            {
                TempData["BatchError"] = "Product not found";
                return RedirectToAction("Index");
            }

            // Create new batch
            var newBatch = new ProductBatch
            {
                ProductId = model.ProductId,
                Quantity = model.Quantity,
                PurchasePrice = model.PurchasePrice,
                ManufacturedDate = model.ManufacturedDate,
                ExpiryDate = model.ExpiryDate,
                AddedDate = DateTime.UtcNow
            };

            // Update product quantity
            product.Quantity += model.Quantity;

            _context.ProductBatches.Add(newBatch);
            await _context.SaveChangesAsync();

            TempData["BatchSuccess"] = "Batch added successfully!";
            return RedirectToAction("ManageBatches", new { id = model.ProductId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteBatch(int batchId, int productId)
        {
            if (batchId <= 0 || productId <= 0)
            {
                TempData["BatchError"] = "Invalid parameters";
                return RedirectToAction("Index");
            }

            var batch = await _context.ProductBatches.FindAsync(batchId);
            if (batch == null)
            {
                TempData["BatchError"] = "Batch not found";
                return RedirectToAction("ManageBatches", new { id = productId });
            }

            var product = await _context.Products.FindAsync(productId);
            if (product == null)
            {
                TempData["BatchError"] = "Product not found";
                return RedirectToAction("Index");
            }

            // Update product quantity
            product.Quantity -= batch.Quantity;

            _context.ProductBatches.Remove(batch);
            await _context.SaveChangesAsync();

            TempData["BatchSuccess"] = "Batch deleted successfully!";
            return RedirectToAction("ManageBatches", new { id = productId });
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.UnitMeasure)
                .Include(p => p.Batches)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (product == null) return NotFound();

            return View(product);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.UnitMeasure)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (product == null) return NotFound();

            return View(product);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products
                .Include(p => p.Batches)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null) return NotFound();

            // Delete associated image
            if (!string.IsNullOrEmpty(product.ImagePath))
            {
                DeleteImageFile(product.ImagePath);
            }

            _context.ProductBatches.RemoveRange(product.Batches);
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

        private async Task<string> SaveImageFile(IFormFile imageFile)
        {
            var uploadsFolder = Path.Combine(_hostingEnvironment.WebRootPath, "images");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(imageFile.FileName);
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(fileStream);
            }

            return "/images/" + uniqueFileName;
        }

        private void DeleteImageFile(string imagePath)
        {
            if (!string.IsNullOrEmpty(imagePath))
            {
                var fullPath = Path.Combine(_hostingEnvironment.WebRootPath, imagePath.TrimStart('/'));
                if (System.IO.File.Exists(fullPath))
                {
                    System.IO.File.Delete(fullPath);
                }
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProductEditVM model, IFormFile imageFile)
        {
            if (id != model.Id) return NotFound();

            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            // Handle image upload
            if (imageFile != null && imageFile.Length > 0)
            {
                // Validate file type and size
                var validTypes = new[] { "image/jpeg", "image/png", "image/gif" };
                if (!validTypes.Contains(imageFile.ContentType.ToLower()))
                {
                    ModelState.AddModelError("imageFile", "Only JPG, PNG or GIF images are allowed");
                }
                else if (imageFile.Length > 5 * 1024 * 1024) // 5MB
                {
                    ModelState.AddModelError("imageFile", "The image must be less than 5MB");
                }
                else
                {
                    // Delete old image if exists
                    if (!string.IsNullOrEmpty(product.ImagePath))
                    {
                        DeleteImageFile(product.ImagePath);
                    }
                    model.ImagePath = await SaveImageFile(imageFile);
                }
            }
            else
            {
                // Preserve existing image
                model.ImagePath = product.ImagePath;
            }

            if (ModelState.IsValid)
            {
                // Update product properties
                product.Name = model.Name;
                product.Description = model.Description;
                product.CategoryId = model.CategoryId;
                product.UnitMeasureId = model.UnitMeasureId;
                product.ReorderLevel = model.ReorderLevel;
                product.ImagePath = model.ImagePath;

                try
                {
                    _context.Update(product);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.Id)) return NotFound();
                    throw;
                }
            }

            // If we got here, something went wrong
            ViewData["ExistingImagePath"] = product.ImagePath;
            PopulateDropdowns();
            return View(model);
        }
    }
}
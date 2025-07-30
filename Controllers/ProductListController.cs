using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ClosedXML.Excel;
using Inventory.Utility;
using InventorySolution.Data;
using InventorySolution.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Controllers
{
    public class ProductListController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductListController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string searchString)
        {
            // Get all products with related data
            var products = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.UnitMeasure)
                .ToListAsync();

            // Create a copy to preserve original order
            var sortedProducts = new List<Product>(products);

            // Sort by name using MergeSort
            MergeSorter.Sort(sortedProducts, "Name", true);

            // Apply search if provided
            if (!string.IsNullOrEmpty(searchString))
            {
                // Use binary search to find products by name
                sortedProducts = BinarySearcher.SearchByName(sortedProducts, searchString);
            }

            return View(sortedProducts);
        }

        public async Task<IActionResult> ExportToExcel()
        {
            // Get all products with related data
            var products = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.UnitMeasure)
                .ToListAsync();

            // Create a copy to preserve original order
            var sortedProducts = new List<Product>(products);

            // Sort by name using MergeSort
            MergeSorter.Sort(sortedProducts, "Name", true);

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Products");
                var currentRow = 1;

                // Header
                worksheet.Cell(currentRow, 1).Value = "Name";
                worksheet.Cell(currentRow, 2).Value = "Price (Rs.)";
                worksheet.Cell(currentRow, 3).Value = "Quantity";
                worksheet.Cell(currentRow, 4).Value = "Expiry Date";
                worksheet.Cell(currentRow, 5).Value = "Manufacture Date";
                worksheet.Cell(currentRow, 6).Value = "Category";
                worksheet.Cell(currentRow, 7).Value = "Unit";

                // Apply header styling
                var headerRange = worksheet.Range(1, 1, 1, 7);
                headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#2a1b3d");
                headerRange.Style.Font.FontColor = XLColor.White;
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                // Body
                foreach (var product in sortedProducts)
                {
                    currentRow++;
                    worksheet.Cell(currentRow, 1).Value = product.Name;
                    worksheet.Cell(currentRow, 2).Value = product.Price;
                    worksheet.Cell(currentRow, 3).Value = product.Quantity;
                    worksheet.Cell(currentRow, 4).Value = product.ExpiryDate.ToString("yyyy-MM-dd");
                    worksheet.Cell(currentRow, 5).Value = product.ManufacturedDate.ToString("yyyy-MM-dd");
                    worksheet.Cell(currentRow, 6).Value = product.Category?.Name ?? "N/A";
                    worksheet.Cell(currentRow, 7).Value = product.UnitMeasure?.Name ?? "N/A";

                    // Highlight low stock
                    if (product.Quantity < product.ReorderLevel)
                    {
                        worksheet.Cell(currentRow, 3).Style.Fill.BackgroundColor = XLColor.Yellow;
                    }

                    // Highlight expired products
                    if (product.ExpiryDate < DateTime.Today)
                    {
                        worksheet.Cell(currentRow, 4).Style.Fill.BackgroundColor = XLColor.Coral;
                    }
                }

                // Auto-fit columns
                worksheet.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                               $"ProductList_{DateTime.Now:yyyyMMddHHmmss}.xlsx");
                }
            }
        }
    }
}
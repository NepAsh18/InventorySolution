using System.Linq;
using System.Threading.Tasks;
using InventorySolution.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventorySolution.Controllers
{
    public class CustomerCategoryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CustomerCategoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Index", "CustomerProduct");
            }

            var category = await _context.Categories
                .Include(c => c.Products)
                .ThenInclude(p => p.UnitMeasure)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
            {
                return NotFound();
            }

            ViewBag.CategoryName = category.Name;
            return View(category.Products.Where(p => p.Quantity > 0).ToList());
        }
    }
}
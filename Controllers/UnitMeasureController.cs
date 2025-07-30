// Controllers/UnitMeasuresController.cs
using InventorySolution.Data;
using InventorySolution.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Inventory.Controllers
{
    public class UnitMeasuresController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UnitMeasuresController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: UnitMeasures
        public async Task<IActionResult> Index()
        {
            return View(await _context.UnitMeasures.ToListAsync());
        }

        // GET: UnitMeasures/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: UnitMeasures/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name")] UnitMeasure unitMeasure)
        {
            // Normalize and trim input
            unitMeasure.Name = unitMeasure.Name?.Trim();

            // Check for existing unit measures (case-insensitive)
            if (unitMeasure.Name != null &&
                await _context.UnitMeasures.AnyAsync(um =>
                    EF.Functions.Collate(um.Name, "SQL_Latin1_General_CP1_CI_AS") == unitMeasure.Name))
            {
                ModelState.AddModelError("Name", "Unit measure with this name already exists.");
            }
            if (ModelState.IsValid)
            {
                _context.Add(unitMeasure);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(unitMeasure);
        }

        // GET: UnitMeasures/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var unitMeasure = await _context.UnitMeasures.FindAsync(id);
            if (unitMeasure == null) return NotFound();

            return View(unitMeasure);
        }

        // POST: UnitMeasures/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name")] UnitMeasure unitMeasure)
        {
            if (id != unitMeasure.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(unitMeasure);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UnitMeasureExists(unitMeasure.Id)) return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(unitMeasure);
        }

        // GET: UnitMeasures/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var unitMeasure = await _context.UnitMeasures
                .FirstOrDefaultAsync(m => m.Id == id);
            if (unitMeasure == null) return NotFound();

            return View(unitMeasure);
        }

        // POST: UnitMeasures/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var unitMeasure = await _context.UnitMeasures.FindAsync(id);
            _context.UnitMeasures.Remove(unitMeasure);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UnitMeasureExists(int id)
        {
            return _context.UnitMeasures.Any(e => e.Id == id);
        }
    }
}
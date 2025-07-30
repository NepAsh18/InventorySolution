using InventorySolution.Data;
using InventorySolution.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InventorySolution.Services
{
    public class SearchService : ISearchService
    {
        private readonly ApplicationDbContext _context;

        public SearchService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Product>> SearchProductsByNameAsync(string searchString)
        {
            if (string.IsNullOrWhiteSpace(searchString))
                return await _context.Products
                    .Include(p => p.Category)
                    .Include(p => p.UnitMeasure)
                    .ToListAsync();

            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.UnitMeasure)
                .Where(p => p.Name.Contains(searchString))
                .ToListAsync();
        }
    }
}
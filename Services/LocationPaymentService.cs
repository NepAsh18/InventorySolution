using InventorySolution.Data;
using InventorySolution.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace InventorySolution.Services
{
    public class LocationPaymentService
    {
        private readonly ApplicationDbContext _context;

        public LocationPaymentService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<double> GetLocationPrice(int locationId)
        {
            var location = await _context.Locations.FindAsync(locationId);
            return location?.Price ?? 0; // Returns 0 if location not found
        }
    }
}
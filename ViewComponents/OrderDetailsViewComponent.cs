// ViewComponents/OrderDetailsViewComponent.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InventorySolution.Data;
using InventorySolution.Models.Entities;

namespace InventorySolution.ViewComponents
{
    public class OrderDetailsViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;

        public OrderDetailsViewComponent(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync(int id)
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
                return Content("Order not found");
            }

            // Use the explicit path to the partial view
            return View("~/Views/Status/_OrderDetailsPartial.cshtml", order);
        }
    }
}
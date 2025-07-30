using System.Collections.Generic;
using System.Linq;
using InventorySolution.Models.Entities;

namespace Inventory.Utility
{
    public class StockAlertItem
    {
        public Product Product { get; set; }
        public int StockDeficit => Product.ReorderLevel - Product.Quantity;
        public double Criticality => (double)StockDeficit / Product.ReorderLevel;
    }

    public class StockAlertService
    {
        public List<StockAlertItem> GetCriticalAlerts(List<Product> products, int count = 5)
        {
            return products
                .Where(p => p.Quantity < p.ReorderLevel)
                .OrderByDescending(p => (double)(p.ReorderLevel - p.Quantity) / p.ReorderLevel)
                .Take(count)
                .Select(p => new StockAlertItem { Product = p })
                .ToList();
        }
    }
}
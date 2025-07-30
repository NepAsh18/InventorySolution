using System.Collections.Generic;
using System.Linq;

namespace InventorySolution.Models.CustomerView
{
    public class Cart
    {
       
        public List<CartItem> Items { get; set; } = new List<CartItem>();
        public decimal Total { get; set; }
        public int ItemCount => Items.Sum(i => i.Quantity);
        public void CalculateTotal()
        {
            Total = Items.Sum(i => i.FinalPrice * i.Quantity);
        }
    }


    public class CartItem
    {
        public int ProductId { get; set; }
        public string Name { get; set; }
        public string ImagePath { get; set; }
        public int Quantity { get; set; }
        public decimal BasePrice { get; set; }
        public decimal FinalPrice { get; set; }
        public decimal Total => FinalPrice * Quantity;
    }
}
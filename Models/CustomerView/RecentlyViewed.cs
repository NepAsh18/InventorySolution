using System;
using InventorySolution.Models.Entities;

namespace InventorySolution.Models.CustomerView
{
    public class RecentlyViewed
    {
        public int Id { get; set; }

        // Foreign key for AppUser
        public string UserId { get; set; }
        public AppUser User { get; set; }

        // Foreign key for Product
        public int ProductId { get; set; }
        public Product Product { get; set; }

        public DateTime ViewedOn { get; set; } // Added missing property
    }
}
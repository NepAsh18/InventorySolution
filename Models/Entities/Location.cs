namespace InventorySolution.Models.Entities
{
    public class Location
    {
        public int Id { get; set; }
        public string Name { get; set; } // e.g., "Warehouse A", "Shelf 5B"
        public string Address => Name;
        public double Price { get; set; } // Use Rs.
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}

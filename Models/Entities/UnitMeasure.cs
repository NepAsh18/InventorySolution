using System.ComponentModel.DataAnnotations;

namespace InventorySolution.Models.Entities
{
    public class UnitMeasure
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } // e.g., "Kg", "Piece", "Liter"

        public ICollection<Product> Products { get; set; }
    }

}

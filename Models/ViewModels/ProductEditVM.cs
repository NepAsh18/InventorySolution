using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace InventorySolution.Models.ViewModels
{
    public class ProductEditVM
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public string Description { get; set; }
        [Required]
        public int CategoryId { get; set; }
        [Required]
        public int UnitMeasureId { get; set; }
        public int ReorderLevel { get; set; }
        public string ImagePath { get; set; }
        [Required]
        public decimal BasePrice { get; set; }

        public int InitialQuantity { get; set; }
        public int CurrentQuantity { get; set; }

        public List<BatchVM> Batches { get; set; } = new List<BatchVM>();
        public BatchVM NewBatch { get; set; } = new BatchVM();
    }
}
using System;
using System.Collections.Generic;

namespace InventorySolution.Models.ViewModels
{
    public class ProductBatchesVM
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int CurrentQuantity { get; set; }
        public int ReorderLevel { get; set; }
        public List<BatchVM> Batches { get; set; } = new List<BatchVM>();
        public BatchVM NewBatch { get; set; } = new BatchVM();
    }
}
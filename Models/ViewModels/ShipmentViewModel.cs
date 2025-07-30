using InventorySolution.Models.Entities;
using System;
using System.Collections.Generic;

namespace InventorySolution.Models.ViewModels
{
    public class ShipmentViewModel
    {
        public int OrderId { get; set; }
        public string CustomerName { get; set; }
        public string CustomerLocation { get; set; }
        public OrderStatus Status { get; set; }
        public DateTime OrderDate { get; set; }
        public string Warehouse { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime? LastStatusChange { get; set; }
        public int ProgressPercentage { get; set; }
    }

    public class ShipmentDetailsViewModel : ShipmentViewModel
    {
        public List<ShipmentItemViewModel> Items { get; set; }
        public DateTime EstimatedDelivery { get; set; }
        public List<string> RoutePoints { get; set; }
    }

    public class ShipmentItemViewModel
    {
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}
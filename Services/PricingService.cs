namespace Inventory.Services
{
    public class PricingService
    {
        public decimal CalculateDynamicPrice(int orderQuantity, decimal baseUnitPrice)
        {
            // Apply quantity-based discounts
            if (orderQuantity >= 1000)
            {
                return baseUnitPrice * 0.8m; // 20% discount for bulk orders
            }
            else if (orderQuantity >= 500)
            {
                return baseUnitPrice * 0.85m; // 15% discount
            }
            else if (orderQuantity >= 100)
            {
                return baseUnitPrice * 0.9m; // 10% discount
            }
            else if (orderQuantity >= 50)
            {
                return baseUnitPrice * 0.95m; // 5% discount
            }

            return baseUnitPrice;
        }
    }
}
using System;
using System.Collections.Generic;
using InventorySolution.Models.Entities;

namespace Inventory.Utility
{
    public static class BinarySearcher
    {
        public static List<Product> SearchByName(List<Product> products, string name)
        {
            if (products == null || products.Count == 0 || string.IsNullOrEmpty(name))
                return new List<Product>();

            // First sort by name for binary search
            QuickSorter.Sort(products, "Name", true);

            var results = new List<Product>();
            int index = BinarySearchByName(products, name, 0, products.Count - 1);

            if (index >= 0)
            {
                // Add the found product
                results.Add(products[index]);

                // Check left for duplicates
                int left = index - 1;
                while (left >= 0 && string.Equals(products[left].Name, name, StringComparison.OrdinalIgnoreCase))
                {
                    results.Add(products[left]);
                    left--;
                }

                // Check right for duplicates
                int right = index + 1;
                while (right < products.Count && string.Equals(products[right].Name, name, StringComparison.OrdinalIgnoreCase))
                {
                    results.Add(products[right]);
                    right++;
                }
            }

            return results;
        }

        private static int BinarySearchByName(List<Product> products, string name, int left, int right)
        {
            while (left <= right)
            {
                int mid = left + (right - left) / 2;
                int comparison = string.Compare(products[mid].Name, name, StringComparison.OrdinalIgnoreCase);

                if (comparison == 0)
                    return mid;

                if (comparison < 0)
                    left = mid + 1;
                else
                    right = mid - 1;
            }

            return -1;
        }
    }
}
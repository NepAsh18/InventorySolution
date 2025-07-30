using System;
using System.Collections.Generic;
using InventorySolution.Models.Entities;

namespace Inventory.Utility
{
    public static class QuickSorter
    {
        public static void Sort(List<Product> products, string sortBy, bool ascending = true)
        {
            if (products == null || products.Count <= 1)
                return;

            QuickSort(products, 0, products.Count - 1, sortBy, ascending);
        }

        private static void QuickSort(List<Product> products, int low, int high, string sortBy, bool ascending)
        {
            if (low < high)
            {
                int pi = Partition(products, low, high, sortBy, ascending);
                QuickSort(products, low, pi - 1, sortBy, ascending);
                QuickSort(products, pi + 1, high, sortBy, ascending);
            }
        }

        private static int Partition(List<Product> products, int low, int high, string sortBy, bool ascending)
        {
            Product pivot = products[high];
            int i = low - 1;

            for (int j = low; j < high; j++)
            {
                if (CompareProducts(products[j], pivot, sortBy, ascending) <= 0)
                {
                    i++;
                    Swap(products, i, j);
                }
            }

            Swap(products, i + 1, high);
            return i + 1;
        }

        private static void Swap(List<Product> products, int i, int j)
        {
            (products[j], products[i]) = (products[i], products[j]);
        }

        private static int CompareProducts(Product a, Product b, string sortBy, bool ascending)
        {
            int comparison = sortBy switch
            {
                "Name" => string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase),
                "ManufacturedDate" => a.ManufacturedDate.CompareTo(b.ManufacturedDate),
                "Id" => a.Id.CompareTo(b.Id),
                _ => 0
            };

            return ascending ? comparison : -comparison;
        }
    }
}
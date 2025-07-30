using System;
using System.Collections.Generic;
using InventorySolution.Models.Entities;

namespace Inventory.Utility
{
    public static class MergeSorter
    {
        public static void Sort(List<Product> products, string sortBy, bool ascending = true)
        {
            if (products == null || products.Count <= 1)
                return;

            MergeSort(products, 0, products.Count - 1, sortBy, ascending);
        }

        private static void MergeSort(List<Product> products, int left, int right, string sortBy, bool ascending)
        {
            if (left < right)
            {
                int mid = left + (right - left) / 2;
                MergeSort(products, left, mid, sortBy, ascending);
                MergeSort(products, mid + 1, right, sortBy, ascending);
                Merge(products, left, mid, right, sortBy, ascending);
            }
        }

        private static void Merge(List<Product> products, int left, int mid, int right, string sortBy, bool ascending)
        {
            int n1 = mid - left + 1;
            int n2 = right - mid;

            List<Product> leftArr = new List<Product>();
            List<Product> rightArr = new List<Product>();

            for (int i = 0; i < n1; i++)
                leftArr.Add(products[left + i]);
            for (int j = 0; j < n2; j++)
                rightArr.Add(products[mid + 1 + j]);

            int iIndex = 0, jIndex = 0;
            int k = left;

            while (iIndex < n1 && jIndex < n2)
            {
                if (CompareProducts(leftArr[iIndex], rightArr[jIndex], sortBy, ascending) <= 0)
                {
                    products[k] = leftArr[iIndex];
                    iIndex++;
                }
                else
                {
                    products[k] = rightArr[jIndex];
                    jIndex++;
                }
                k++;
            }

            while (iIndex < n1)
            {
                products[k] = leftArr[iIndex];
                iIndex++;
                k++;
            }

            while (jIndex < n2)
            {
                products[k] = rightArr[jIndex];
                jIndex++;
                k++;
            }
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
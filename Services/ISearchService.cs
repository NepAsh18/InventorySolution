using InventorySolution.Models.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InventorySolution.Services
{
    public interface ISearchService
    {
        Task<IEnumerable<Product>> SearchProductsByNameAsync(string searchString);
    }
}
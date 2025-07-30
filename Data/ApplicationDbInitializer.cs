using InventorySolution.Data;
using InventorySolution.Models.Entities;

namespace Inventory.Data
{
    public class ApplicationDbInitializer
    {
        public static void Seed(IApplicationBuilder applicationBuilder)
        {
            using (var servicescope = applicationBuilder.ApplicationServices.CreateScope())
            {
                var context = servicescope.ServiceProvider.GetService<ApplicationDbContext>(); 
                context.Database.EnsureCreated();
                //Category
                if(!context.Categories.Any())
                {
                    context.Categories.AddRange(new List<Category>()
                    {
                        new()
                        {
                            Name = "Etable"
                        },
                        new()
                        {
                            Name = "Non-Etable"
                        }
                    });
                }
            }
        }
    }
}

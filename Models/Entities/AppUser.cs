// In Models/Entities/AppUser.cs
using Microsoft.AspNetCore.Identity;

namespace InventorySolution.Models.Entities
{
    public class AppUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public string SecurityQuestion { get; set; } = string.Empty;
        public string SecurityAnswer { get; set; } = string.Empty;
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    }
}
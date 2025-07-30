// Controllers/AdminController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventorySolution.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
       
        public IActionResult Dashboard()
        {
            // Get user ID from session
            var userId = HttpContext.Session.GetString("UserId");

            // Get user roles from session
            var roles = HttpContext.Session.GetString("UserRoles")?.Split(',') ?? Array.Empty<string>();

            return View();
        }
    }
}
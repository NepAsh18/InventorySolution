using InventorySolution.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace InventorySolution.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View(); // This will show the animated welcome screen
        }

        public IActionResult Privacy()
        {
            return View(); // Keep existing privacy page
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
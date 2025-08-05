using InventorySolution.Data;
using InventorySolution.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Globalization;
using System.Net.Http.Json;

namespace InventorySolution.Controllers
{
    public class ForecastController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IConfiguration _configuration;

        public ForecastController(ApplicationDbContext db, IConfiguration configuration)
        {
            _db = db;
            _configuration = configuration;
        }

        public async Task<IActionResult> Index()
        {
            // Using original OrderStatus.Delivered as completed orders
            var startDate = DateTime.Today.AddDays(-180);
            var orders = await _db.Orders
                .Where(o => o.OrderDate >= startDate && o.Status == OrderStatus.Delivered)
                .ToListAsync();

            // Prepare historical data using original Order model
            var historicalData = orders
                .GroupBy(o => o.OrderDate.Date)
                .Select(g => new
                {
                    date = g.Key.ToString("yyyy-MM-dd"),
                    amount = g.Sum(o => o.TotalAmount)
                })
                .ToList();

            var forecast = new List<SalesForecast>();
            var apiUrl = _configuration["ForecastApiUrl"] ?? "http://localhost:5000/forecast";

            using (var client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromMinutes(3);

                try
                {
                    var response = await client.PostAsJsonAsync(apiUrl, new { data = historicalData });

                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        forecast = JsonConvert.DeserializeObject<List<SalesForecast>>(content);
                    }
                    else
                    {
                        ViewBag.Error = "Forecast service returned error: " +
                                       await response.Content.ReadAsStringAsync();
                    }
                }
                catch (Exception ex)
                {
                    ViewBag.Error = "Forecast service unavailable: " + ex.Message;
                }
            }

            return View(forecast);
        }

        public async Task<IActionResult> LongTerm()
        {
            // Get last 6 months of sales data (by month)
            var startDate = DateTime.Today.AddMonths(-6).Date;
            var orders = await _db.Orders
                .Where(o => o.OrderDate >= startDate && o.Status == OrderStatus.Delivered)
                .ToListAsync();

            // Group by month
            var historicalData = orders
                .GroupBy(o => new { o.OrderDate.Year, o.OrderDate.Month })
                .Select(g => new
                {
                    date = $"{g.Key.Year}-{g.Key.Month:00}",
                    amount = g.Sum(o => o.TotalAmount)
                })
                .OrderBy(x => x.date)
                .ToList();

            var forecast = new List<SalesForecast>();
            var apiUrl = _configuration["ForecastApiUrl"] ?? "http://localhost:5000/forecast";

            using (var client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromMinutes(3);

                try
                {
                    var response = await client.PostAsJsonAsync(apiUrl, new { data = historicalData });

                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        forecast = JsonConvert.DeserializeObject<List<SalesForecast>>(content);
                    }
                    else
                    {
                        ViewBag.Error = "Forecast service returned error: " +
                                       await response.Content.ReadAsStringAsync();
                    }
                }
                catch (Exception ex)
                {
                    ViewBag.Error = "Forecast service unavailable: " + ex.Message;
                }
            }

            // Prepare view model
            var model = new LongTermForecastViewModel
            {
                Historical = historicalData.Select(d => new MonthlySales
                {
                    Label = DateTime.ParseExact(d.date, "yyyy-MM", CultureInfo.InvariantCulture)
                                .ToString("MMM yyyy"),
                    Value = d.amount
                }).ToList(),

                ForecastNext6Months = forecast.Take(6).Select(f => new MonthlySales
                {
                    Label = f.Date.ToString("MMM yyyy"),
                    Value = f.Amount
                }).ToList(),

                ForecastNextYear = forecast.Skip(6).Take(12).Select(f => new MonthlySales
                {
                    Label = f.Date.ToString("MMM yyyy"),
                    Value = f.Amount
                }).ToList()
            };

            return View("LongTerm", model);
        }

        // Nested classes
        public class SalesForecast
        {
            public DateTime Date { get; set; }
            public decimal Amount { get; set; }
        }

        public class LongTermForecastViewModel
        {
            public List<MonthlySales> Historical { get; set; } = new List<MonthlySales>();
            public List<MonthlySales> ForecastNext6Months { get; set; } = new List<MonthlySales>();
            public List<MonthlySales> ForecastNextYear { get; set; } = new List<MonthlySales>();
        }

        public class MonthlySales
        {
            public string Label { get; set; }
            public decimal Value { get; set; }
        }
    }
}
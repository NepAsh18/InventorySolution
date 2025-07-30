using ClosedXML.Excel;
using InventorySolution.Data;
using InventorySolution.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Globalization;

namespace InventorySolution.Controllers
{
    public class ReportController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Orders(int? year, int? month)
        {
            year ??= DateTime.Now.Year;
            month ??= DateTime.Now.Month;

            var orders = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.Location)
                .Where(o => o.OrderDate.Year == year && o.OrderDate.Month == month)
                .OrderByDescending(o => o.OrderDate)
                .Select(o => new OrderReportViewModel
                {
                    Id = o.Id,
                    UserName = o.User.UserName,
                    LocationName = o.Location.Name,
                    OrderDate = o.OrderDate,
                    TotalAmount = o.TotalAmount,
                    Status = o.Status
                })
                .AsNoTracking()
                .ToListAsync();

            ViewBag.SelectedYear = year;
            ViewBag.SelectedMonth = month;
            ViewBag.Years = await GetAvailableYears();
            ViewBag.ReportType = "Orders";

            return View("ReportView", orders);
        }

        public async Task<IActionResult> Shipments(int? year, int? month)
        {
            year ??= DateTime.Now.Year;
            month ??= DateTime.Now.Month;

            var shipments = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.Location)
                .Where(o => o.LastStatusChange.Year == year &&
                            o.LastStatusChange.Month == month &&
                            (o.Status == OrderStatus.Shipped || o.Status == OrderStatus.Delivered))
                .OrderByDescending(o => o.LastStatusChange)
                .Select(o => new ShipmentReportViewModel
                {
                    OrderId = o.Id,
                    CustomerName = o.User.UserName,
                    CustomerLocation = o.Location.Name,
                    Status = o.Status,
                    OrderDate = o.OrderDate,
                    TotalAmount = o.TotalAmount,
                    LastStatusChange = o.LastStatusChange
                })
                .AsNoTracking()
                .ToListAsync();

            ViewBag.SelectedYear = year;
            ViewBag.SelectedMonth = month;
            ViewBag.Years = await GetAvailableYears();
            ViewBag.ReportType = "Shipments";

            return View("ReportView", shipments);
        }

        private async Task<List<int>> GetAvailableYears()
        {
            var orderYears = await _context.Orders
                .Select(o => o.OrderDate.Year)
                .Distinct()
                .ToListAsync();

            var shipmentYears = await _context.Orders
                .Where(o => o.Status == OrderStatus.Shipped || o.Status == OrderStatus.Delivered)
                .Select(o => o.LastStatusChange.Year)
                .Distinct()
                .ToListAsync();

            return orderYears.Union(shipmentYears).OrderByDescending(y => y).ToList();
        }

        public async Task<IActionResult> Export(string reportType, int year, int month)
        {
            DataTable dt = new DataTable();

            if (reportType == "Orders")
            {
                dt.TableName = $"Orders_{year}_{month}";
                dt.Columns.AddRange(new DataColumn[] {
                    new("Order ID", typeof(int)),
                    new("Customer", typeof(string)),
                    new("Location", typeof(string)),
                    new("Order Date", typeof(DateTime)),
                    new("Total Amount", typeof(decimal)),
                    new("Status", typeof(string))
                });

                var data = await _context.Orders
                    .Include(o => o.User)
                    .Include(o => o.Location)
                    .Where(o => o.OrderDate.Year == year && o.OrderDate.Month == month)
                    .ToListAsync();

                foreach (var item in data)
                {
                    dt.Rows.Add(
                        item.Id,
                        item.User?.UserName,
                        item.Location?.Name,
                        item.OrderDate,
                        item.TotalAmount,
                        item.Status.ToString()
                    );
                }
            }
            else if (reportType == "Shipments")
            {
                dt.TableName = $"Shipments_{year}_{month}";
                dt.Columns.AddRange(new DataColumn[] {
                    new("Order ID", typeof(int)),
                    new("Customer", typeof(string)),
                    new("Location", typeof(string)),
                    new("Status", typeof(string)),
                    new("Order Date", typeof(DateTime)),
                    new("Total Amount", typeof(decimal)),
                    new("Last Update", typeof(DateTime))
                });

                var data = await _context.Orders
                    .Include(o => o.User)
                    .Include(o => o.Location)
                    .Where(o => o.LastStatusChange.Year == year &&
                                o.LastStatusChange.Month == month &&
                                (o.Status == OrderStatus.Shipped || o.Status == OrderStatus.Delivered))
                    .ToListAsync();

                foreach (var item in data)
                {
                    dt.Rows.Add(
                        item.Id,
                        item.User?.UserName,
                        item.Location?.Name,
                        item.Status.ToString(),
                        item.OrderDate,
                        item.TotalAmount,
                        item.LastStatusChange
                    );
                }
            }
            else
            {
                return BadRequest("Invalid report type");
            }

            using (var workbook = new XLWorkbook())
            {
                workbook.Worksheets.Add(dt);
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return File(stream.ToArray(),
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        $"{dt.TableName}.xlsx");
                }
            }
        }
    }

    public class OrderReportViewModel
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string LocationName { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public OrderStatus Status { get; set; }
    }

    public class ShipmentReportViewModel
    {
        public int OrderId { get; set; }
        public string CustomerName { get; set; }
        public string CustomerLocation { get; set; }
        public OrderStatus Status { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime LastStatusChange { get; set; }
    }
}
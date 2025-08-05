using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using DocumentFormat.OpenXml.Spreadsheet;
using Inventory.Controllers;
using Inventory.Services;
using Inventory.Utility;
using InventorySolution.Controllers;
using InventorySolution.Data;

using InventorySolution.Models;

using InventorySolution.Models.Entities;
using InventorySolution.Services;

using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;

namespace InventorySolution
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);


           
            // Add services to the container.
            builder.Services.AddControllersWithViews();
            //Add database configuration
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
     options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
            // Add StockAlertServices
            builder.Services.AddScoped<StockAlertService>();
            //Add ExcelServices
            builder.Services.AddScoped<ProductListController>();
            // Configure Identity with secure settings
            builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequiredLength = 6;
                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedAccount = false;
                options.SignIn.RequireConfirmedEmail = false;
                options.SignIn.RequireConfirmedPhoneNumber = false;
            })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            //Add OrderServices
            builder.Services.AddScoped<IPaymentService, PaymentService>();
            builder.Services.AddScoped<INotificationService, NotificationService>();
            builder.Services.AddScoped<IOrderStatusService, OrderStatusService>();
            builder.Services.AddScoped<LocationPaymentService>();
            builder.Services.AddHostedService<OrderStatusBackgroundService>();
            builder.Services.AddSignalR();
            // Add HTTP context accessor
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddHttpClient();



            //Add Dynamic Pricing
            builder.Services.AddScoped<PricingService>();

            //Add Cookies
            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Account/Login";
                options.AccessDeniedPath = "/Account/Login"; // Redirect to login instead of AccessDenied
                options.LogoutPath = "/Account/Logout";
                options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
                options.SlidingExpiration = true;

                // Add this to pass the return URL with access denied flag
                options.Events = new CookieAuthenticationEvents
                {
                    OnRedirectToAccessDenied = context =>
                    {
                        context.Response.Redirect($"/Account/Login?returnUrl={context.Request.Path}&accessDenied=true");
                        return Task.CompletedTask;
                    }
                };
            });
            // Add this to your services configuration
            builder.Services.AddScoped<ISearchService, SearchService>();

            // Add HttpClient support
            builder.Services.AddHttpClient();
            // Add ConfigurationManager
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);





            var app = builder.Build();



            PythonService.StartApi();
            await Task.Delay(5000);

            //Add Seed Services
            await SeedService.SeedDatabase(app.Services);

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseSession();
           
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseAuthentication();

            app.UseAuthorization();
            app.MapControllerRoute(
    name: "forecast",
    pattern: "forecast",
    defaults: new { controller = "Forecast", action = "Index" });



            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
           

            app.Run();

          
        }
    }
}

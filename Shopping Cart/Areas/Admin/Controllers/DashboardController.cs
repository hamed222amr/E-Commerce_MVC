using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoppingCartApp.Data;
using ShoppingCartApp.ViewModels;
using System.Globalization;

namespace ShoppingCartApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var vm = new AdminDashboardViewModel();

            
            vm.UsersCount = await _context.Users.CountAsync();
            vm.OrdersCount = await _context.Orders.CountAsync();
            vm.TotalRevenue = await _context.Orders.SumAsync(o => (decimal?)o.TotalPrice) ?? 0m;

            // recent orders (last 10)
            vm.RecentOrders = await _context.Orders
                .OrderByDescending(o => o.CreatedAt)
                .Take(10)
                .Select(o => new RecentOrderVm
                {
                    Id = o.Id,
                    UserEmail = o.User != null ? o.User.Email : o.UserId,
                    CreatedAt = o.CreatedAt,
                    TotalPrice = o.TotalPrice,
                    Status = o.Status
                }).ToListAsync();

            // top products by quantity sold (join order items)
            vm.TopProducts = await _context.OrderItems
                .GroupBy(oi => new { oi.ProductId, oi.Product.Name })
                .Select(g => new TopProductVm
                {
                    ProductId = g.Key.ProductId,
                    Name = g.Key.Name,
                    SoldQuantity = g.Sum(x => x.Quantity),
                    Revenue = g.Sum(x => x.Quantity * x.UnitPrice)
                })
                .OrderByDescending(x => x.SoldQuantity)
                .Take(8)
                .ToListAsync();

            // recent users (last 8)
            vm.RecentUsers = await _context.Users
                .OrderByDescending(u => u.Id) // or by Created date if you have it
                .Take(8)
                .Select(u => new UserVm
                {
                    Id = u.Id,
                    Email = u.Email,
                    RegisteredAt = u.LockoutEnd.HasValue ?
               u.LockoutEnd.Value.UtcDateTime : DateTime.MinValue

                    // replace if you have CreatedAt
                })
                .ToListAsync();

            // chart data: revenue per day for last 7 days
            var days = Enumerable.Range(0, 7)
                .Select(i => DateTime.UtcNow.Date.AddDays(-i))
                .Reverse()
                .ToList();

            vm.ChartLabels = days;
            var values = new List<decimal>();
            foreach (var d in days)
            {
                var dayTotal = await _context.Orders
                    .Where(o => o.CreatedAt.Date == d.Date)
                    .SumAsync(o => (decimal?)o.TotalPrice) ?? 0m;
                values.Add(dayTotal);
            }
            vm.ChartValues = values;

            return View(vm);
        }
    }
}

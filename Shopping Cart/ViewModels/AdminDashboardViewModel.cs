// ViewModels/AdminDashboardViewModel.cs
using System;
using System.Collections.Generic;

namespace ShoppingCartApp.ViewModels
{
    public class AdminDashboardViewModel
    {
        // summary
        public int UsersCount { get; set; }
        public int OrdersCount { get; set; }
        public decimal TotalRevenue { get; set; }

        // lists for tables
        public List<RecentOrderVm> RecentOrders { get; set; } = new();
        public List<TopProductVm> TopProducts { get; set; } = new();
        public List<UserVm> RecentUsers { get; set; } = new();

        // chart (daily revenue for last N days)
        public List<DateTime> ChartLabels { get; set; } = new();
        public List<decimal> ChartValues { get; set; } = new();
    }

    public class RecentOrderVm
    {
        public int Id { get; set; }
        public string UserEmail { get; set; }
        public DateTime CreatedAt { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; }
    }

    public class TopProductVm
    {
        public int ProductId { get; set; }
        public string Name { get; set; }
        public int SoldQuantity { get; set; }
        public decimal Revenue { get; set; }
    }

    public class UserVm
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public DateTime RegisteredAt { get; set; }
    }
}

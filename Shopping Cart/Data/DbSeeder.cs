using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ShoppingCartApp.Models;

namespace ShoppingCartApp.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var services = scope.ServiceProvider;

            var context = services.GetRequiredService<ApplicationDbContext>();
            await context.Database.MigrateAsync();

            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            if (!await roleManager.RoleExistsAsync("Admin"))
            {
                await roleManager.CreateAsync(new IdentityRole("Admin"));
            }
            if (!await roleManager.RoleExistsAsync("User"))
            {
                await roleManager.CreateAsync(new IdentityRole("User"));
            }

            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var adminEmail = "admin@shop.local";
            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var admin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "Site Admin"
                };
                await userManager.CreateAsync(admin, "Admin123!");
                await userManager.AddToRoleAsync(admin, "Admin");
            }

            // seed categories
            if (!context.Categories.Any())
            {
                context.Categories.AddRange(
                    new Category { Name = "Electronics" },
                    new Category { Name = "Books" },
                    new Category { Name = "Clothing" }
                );
                await context.SaveChangesAsync();
            }

            // seed products (use images placed in wwwroot/images/)
            if (!context.Products.Any())
            {
                var electronicsId = context.Categories.First(c => c.Name == "Electronics").Id;
                var booksId = context.Categories.First(c => c.Name == "Books").Id;

                context.Products.AddRange(
                    new Product { Name = "Wireless Headphones", Description = "High-quality sound", Price = 59.99m, Stock = 50, ImageUrl = "/images/demo1.jpg", CategoryId = electronicsId },
                    new Product { Name = "Programming Book", Description = "Learn ASP.NET Core", Price = 29.99m, Stock = 20, ImageUrl = "/images/demo2.jpg", CategoryId = booksId },
                    new Product { Name = "T-Shirt", Description = "100% cotton", Price = 15.00m, Stock = 100, ImageUrl = "/images/demo3.jpg", CategoryId = context.Categories.First(c => c.Name == "Clothing").Id }
                );
                await context.SaveChangesAsync();
            }
        }
    }
}

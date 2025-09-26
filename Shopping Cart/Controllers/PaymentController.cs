using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoppingCartApp.Data;
using ShoppingCartApp.Helpers;
using ShoppingCartApp.Models;
using Stripe;
using Stripe.Checkout;
using System.Security.Claims;

namespace ShoppingCartApp.Controllers
{
    [Authorize]
    public class PaymentController : Controller
    {
        private readonly IConfiguration _config;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public PaymentController(
            IConfiguration config,
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _config = config;
            _context = context;
            _userManager = userManager;
        }

        public IActionResult Checkout()
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();
            if (!cart.Any()) return RedirectToAction("Index", "Cart");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account");

            var order = new Order
            {
                UserId = userId,
                TotalPrice = cart.Sum(c => c.Price * c.Quantity),
                Status = "New",
                Items = cart.Select(c => new OrderItem
                {
                    ProductId = c.ProductId,
                    Quantity = c.Quantity,
                    UnitPrice = c.Price
                }).ToList()
            };

            _context.Orders.Add(order);
            _context.SaveChanges();

            StripeConfiguration.ApiKey = _config["Stripe:SecretKey"];
            var lineItems = cart.Select(item => new SessionLineItemOptions
            {
                PriceData = new SessionLineItemPriceDataOptions
                {
                    UnitAmountDecimal = item.Price * 100,
                    Currency = "usd",
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = item.Name
                    }
                },
                Quantity = item.Quantity
            }).ToList();

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = lineItems,
                Mode = "payment",
                SuccessUrl = Url.Action("Success", "Payment", new { orderId = order.Id }, Request.Scheme),
                CancelUrl = Url.Action("Cancel", "Payment", new { orderId = order.Id }, Request.Scheme),
            };

            var service = new SessionService();
            var session = service.Create(options);

            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
        }

        public IActionResult Success(int orderId)
        {
            var order = _context.Orders
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                .FirstOrDefault(o => o.Id == orderId);

            if (order != null)
            {
                order.Status = "Processing";

                if (order.Items != null)
                {
                    foreach (var item in order.Items)
                    {
                        if (item.Product != null)
                        {
                            item.Product.Stock -= item.Quantity;
                            if (item.Product.Stock < 0)
                                item.Product.Stock = 0;
                        }
                    }
                }

                _context.SaveChanges();
            }

            HttpContext.Session.Remove("Cart");
            return View();
        }

        public IActionResult Cancel(int orderId)
        {
            var order = _context.Orders.FirstOrDefault(o => o.Id == orderId);
            if (order != null)
            {
                order.Status = "Cancelled";
                _context.SaveChanges();
            }
            return View();
        }

        public async Task<IActionResult> MyOrders()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account");

            var orders = await _context.Orders
                .Include(o => o.Items)
                .ThenInclude(i => i.Product)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            return View(orders);
        }

        [HttpGet]
        public IActionResult Repay(int orderId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var order = _context.Orders
                .Include(o => o.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefault(o => o.Id == orderId && o.UserId == userId && o.Status == "New");

            if (order == null)
                return RedirectToAction("MyOrders");

            // Stripe Checkout
            StripeConfiguration.ApiKey = _config["Stripe:SecretKey"];
            var lineItems = order.Items.Select(item => new SessionLineItemOptions
            {
                PriceData = new SessionLineItemPriceDataOptions
                {
                    UnitAmountDecimal = item.UnitPrice * 100,
                    Currency = "usd",
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = item.Product?.Name ?? "Product"
                    }
                },
                Quantity = item.Quantity
            }).ToList();

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = lineItems,
                Mode = "payment",
                SuccessUrl = Url.Action("Success", "Payment", new { orderId = order.Id }, Request.Scheme),
                CancelUrl = Url.Action("Cancel", "Payment", new { orderId = order.Id }, Request.Scheme),
            };

            var service = new SessionService();
            var session = service.Create(options);

            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
        }
    }
}

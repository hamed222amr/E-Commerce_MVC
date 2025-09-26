using System.Collections.Generic;
using System.Linq;
using ShoppingCartApp.Models;

namespace ShoppingCartApp.ViewModels
{
    public class CheckoutViewModel
    {
        public List<CartItem> Items { get; set; } = new List<CartItem>();

        public decimal Total => Items?.Sum(i => i.Price * i.Quantity) ?? 0;
    }
}

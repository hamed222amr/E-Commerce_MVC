using System.ComponentModel.DataAnnotations.Schema;

namespace ShoppingCartApp.Models
{
    public class OrderItem
    {
        public int Id { get; set; }

        // FK to Order
        public int OrderId { get; set; }
        public Order Order { get; set; }

        // FK to Product
        public int ProductId { get; set; }
        public Product Product { get; set; }

        public int Quantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }
    }
}

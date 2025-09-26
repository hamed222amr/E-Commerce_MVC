using System;

namespace ShoppingCartApp.Models
{
    public class Payment
    {
        public int Id { get; set; }

        public int OrderId { get; set; }
        public Order Order { get; set; }

        public string Method { get; set; }          
        public string Status { get; set; }          
        public string TransactionId { get; set; }   
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

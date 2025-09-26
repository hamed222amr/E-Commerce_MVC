using Microsoft.AspNetCore.Identity;

namespace ShoppingCartApp.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; }
    }
}

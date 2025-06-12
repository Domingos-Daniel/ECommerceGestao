using Microsoft.AspNetCore.Identity;

namespace ECommerceGestao.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string ZipCode { get; set; } = string.Empty;
        public ICollection<Order>? Orders { get; set; }
    }
}

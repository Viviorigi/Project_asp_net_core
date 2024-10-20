using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace btlAspNetCore.Models.DataModels
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }

        public string Password { get; set; }

        public string? Avatar {  get; set; }

        public string Phone { get; set; }
        public string Address { get; set; }
        [DefaultValue(0)]
        public byte Role { get; set; } 
        public ICollection<Order>? Orders { get; set; }
        public ICollection<Rating>? Ratings { get; set; }
        public ICollection<Wishlist>? Wishlists { get; set; }
    }
}

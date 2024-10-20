using System.ComponentModel.DataAnnotations;

namespace btlAspNetCore.Models.DataModels
{
        public class Product
        {
        [Key] // Indicates that this property is the primary key
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        [StringLength(100, ErrorMessage = "Name cannot be longer than 100 characters.")]
        public string Name { get; set; }

        public string? Img { get; set; }

        [Required(ErrorMessage = "Price is required.")]
        [Range(0.01, float.MaxValue, ErrorMessage = "Price must be greater than 0.")]
        public float Price { get; set; }

        [Required(ErrorMessage = "Sale price is required.")]
        [Range(0.01, float.MaxValue, ErrorMessage = "Sale price must be greater than 0.")]
        public float SalePrice { get; set; }

        [Required(ErrorMessage = "Status is required.")]
        [Range(0, 1, ErrorMessage = "Status must be either 0 (Inactive) or 1 (Active).")]
        public byte Status { get; set; }

        public string Description { get; set; }

        [Required(ErrorMessage = "Category Id is required.")]
        public int CategoryId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now; 

        public Category? Category { get; set; }

        public ICollection<ImgProduct>? ImgProducts { get; set; } = new List<ImgProduct>();

        public ICollection<Wishlist>? Wishlists { get; set; } = new List<Wishlist>();

        public ICollection<OrderDetail>? OrderDetails { get; set; } = new List<OrderDetail>();

        public ICollection<Rating>? Ratings { get; set; } = new List<Rating>();
    }
}

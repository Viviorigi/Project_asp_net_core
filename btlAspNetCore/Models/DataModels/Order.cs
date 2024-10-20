using System.ComponentModel.DataAnnotations;

namespace btlAspNetCore.Models.DataModels
{
    public class Order
    {
        [Key]
        public int Id { get; set; }
        public int UserId { get; set; }
        public int Status { get; set; } 
        public string? OrderNote { get; set; }
        public byte PaymentMethod { get; set; }
        public User? User { get; set; }
        public ICollection<OrderDetail>? OrderDetails { get; set; }
    }
}

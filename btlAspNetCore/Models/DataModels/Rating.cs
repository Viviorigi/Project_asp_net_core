namespace btlAspNetCore.Models.DataModels
{
    public class Rating
    {
        public int ProductId { get; set; }
        public int UserId { get; set; }
        public float RatingStar { get; set; }
        public string Comment { get; set; }

        public Product? Product { get; set; }
        public User? User { get; set; }
    }
}

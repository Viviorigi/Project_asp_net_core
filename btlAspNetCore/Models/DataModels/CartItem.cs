namespace btlAspNetCore.Models.DataModels
{
    public class CartItem
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string Image {  get; set; }
        public float Price { get; set; }
        public int Quantity { get; set; }
    }
}

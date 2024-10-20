using System.ComponentModel.DataAnnotations;

namespace btlAspNetCore.Models.DataModels
{
    public class ImgProduct
    {
        [Key]
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string Image { get; set; }
        public Product? Product { get; set; }
    }
}

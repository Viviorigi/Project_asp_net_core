using System.ComponentModel.DataAnnotations;

namespace btlAspNetCore.Models.DataModels
{
    public class Blog
    {
        [Key]
        public int Id { get; set; }
        public string? Image { get; set; }
        public string Title { get; set; }
        public string BlogContent { get; set; }
    }
}

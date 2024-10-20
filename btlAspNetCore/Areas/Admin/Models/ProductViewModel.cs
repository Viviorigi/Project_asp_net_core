using btlAspNetCore.Models.DataModels;
using X.PagedList;

namespace btlAspNetCore.Areas.Admin.Models
{
    public class ProductViewModel
    {
        public IPagedList<Product> Products { get; set; }
        public int? CateId { get; set; } // Add this property
        public string? Name { get; set; }
    }

}

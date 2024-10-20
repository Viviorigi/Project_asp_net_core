using btlAspNetCore.Models.DataModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace btlAspNetCore.Areas.Admin.Controllers
{

    public class HomeController : BaseController
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            int productCount = _context.Products.Count();
            int userCount = _context.Users.Count();
            int orderCount = _context.Orders.Count();
            float totalOrderPrice = _context.OrderDetails.Sum(o => o.TotalPrice);
            ViewBag.ProductCount = productCount;
            ViewBag.UserCount = userCount;
            ViewBag.OrderCount = orderCount;
            ViewBag.TotalOrderPrice = totalOrderPrice.ToString("#,##0");
            ViewBag.LatestUsers = _context.Users
                             .OrderByDescending(u => u.Id) 
                             .Take(5)
                             .ToList();
            ViewBag.LastestOrders = _context.Orders.Include(o => o.OrderDetails)
                .OrderByDescending(u => u.Id) .Take(8)
                .Select(o => new
                {
                    o.Id,
                    o.PaymentMethod,
                    o.Status,
                    TotalPrice = o.OrderDetails.Sum(od => od.TotalPrice)
                }).ToList();
            return View();
        }
    }
}

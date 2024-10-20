using btlAspNetCore.Models;
using btlAspNetCore.Models.DataModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Scripting;
using System.Diagnostics;
using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using X.PagedList.Extensions;
using Microsoft.AspNetCore.Authentication;
using NuGet.Protocol.Plugins;
using System.Security.Claims;
using System.Security.Principal;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;

namespace btlAspNetCore.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;


        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger, AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.Categories= await _context.Categories.Take(6).ToListAsync();
            ViewBag.Banners= await _context.Banners.ToListAsync();
            ViewBag.Products = await _context.Products.ToListAsync();
            List<int> wishlistProducts = new List<int>();
            if (User.Identity.IsAuthenticated)
            {
                int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

                wishlistProducts = await _context.Wishlists
                    .Where(w => w.UserId == userId)
                    .Select(w => w.ProductId)
                    .ToListAsync();
            }
            else
            {
                // Check wishlist in session or cookies for guest users
                var sessionWishlist = HttpContext.Session.GetString("Wishlist");
                if (!string.IsNullOrEmpty(sessionWishlist))
                {
                    wishlistProducts = JsonConvert.DeserializeObject<List<int>>(sessionWishlist);
                }
            }
            ViewBag.WishlistProducts = wishlistProducts;
            return View();
        }

        public async Task<IActionResult> ShopCategory(int cateId = 0, string sort = "", string name = "", int page = 1, int minPrice = 0, int maxPrice = 0)
        {
            // Sort options
            ViewBag.SortOptions = new List<SelectListItem>
    {
        new SelectListItem { Value = "nameAsc", Text = "Name A-Z" },
        new SelectListItem { Value = "nameDesc", Text = "Name Z-A" },
        new SelectListItem { Value = "priceAsc", Text = "Price Low to High" },
        new SelectListItem { Value = "priceDesc", Text = "Price High to Low" }
    };

            ViewBag.SortSelected = sort;
            ViewBag.Name = name;
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;

            // Fetching categories with product count
            ViewBag.Categories = await _context.Categories
                .Select(c => new
                {
                    Category = c,
                    ProductCount = c.Products.Count()
                })
                .ToListAsync();

            // Initialize product query
            var query = _context.Products.Include(p => p.Category).AsQueryable();

            // Apply filters
            if (cateId > 0) query = query.Where(p => p.CategoryId == cateId);
            if (!string.IsNullOrWhiteSpace(name)) query = query.Where(p => p.Name.Contains(name));
            if (minPrice > 0) query = query.Where(p => p.SalePrice >= minPrice);
            if (maxPrice > 0) query = query.Where(p => p.SalePrice <= maxPrice);

            // Apply sorting
            switch (sort)
            {
                case "nameAsc": query = query.OrderBy(p => p.Name); break;
                case "nameDesc": query = query.OrderByDescending(p => p.Name); break;
                case "priceAsc": query = query.OrderBy(p => p.SalePrice); break;
                case "priceDesc": query = query.OrderByDescending(p => p.SalePrice); break;
                default: query = query.OrderBy(p => p.Id); break;
            }

            // Get wishlist products for the current user
            List<int> wishlistProducts = new List<int>();
            if (User.Identity.IsAuthenticated)
            {
                int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

                wishlistProducts = await _context.Wishlists
                    .Where(w => w.UserId == userId)
                    .Select(w => w.ProductId)
                    .ToListAsync();
            }
            else
            {
                // Check wishlist in session or cookies for guest users
                var sessionWishlist = HttpContext.Session.GetString("Wishlist");
                if (!string.IsNullOrEmpty(sessionWishlist))
                {
                    wishlistProducts = JsonConvert.DeserializeObject<List<int>>(sessionWishlist);
                }
            }

            // Paginate the results
            int pageSize = 9;
            var pagedList =  query.ToPagedList(page, pageSize);
            ViewBag.Products = pagedList;
            ViewBag.WishlistProducts = wishlistProducts; // Send to view for wishlist check

            return View(pagedList);
        }


        public async Task<IActionResult> DetailProduct(int proId)
        {
          
            var product = await _context.Products
                .Include(p => p.Category) 
                .Include(p => p.ImgProducts)
                .Include(p => p.Ratings)
                .ThenInclude(r => r.User)
               .FirstOrDefaultAsync(p => p.Id == proId);

            if (product == null)
            {
                return NotFound();
            }
            var ratings = product.Ratings.ToList();
            float averageRating = ratings.Any() ? (float)Math.Round(ratings.Average(r => r.RatingStar), 1) : 0;
            int totalReviews = ratings.Count;

            ViewBag.AverageRating = averageRating;
            ViewBag.TotalReviews = totalReviews;

            if (User.Identity.IsAuthenticated)
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

                ViewBag.WishlistProducts = await _context.Wishlists
                    .Where(w => w.UserId == userId)
                    .Select(w => w.ProductId)
                    .ToListAsync();
            }
            else
            {
          
                ViewBag.WishlistProducts = GetWishlistFromSessionOrCookie();
            }

            return View(product); 
        }

        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(LoginModel model)
        {
            if (ModelState.IsValid)
            {
                var acc = await _context.Users.FirstOrDefaultAsync(a => a.Email == model.Email);

                if (acc == null)
                {
                    ModelState.AddModelError(string.Empty, "Your account does not exist");
                    return View(model); // Pass the model back to the view
                }

                bool isPasswordCorrect = BCrypt.Net.BCrypt.Verify(model.Password, acc.Password);

                if (!isPasswordCorrect)
                {
                    ModelState.AddModelError(string.Empty, "Invalid password");
                    return View(model);
                }

                var identity = new ClaimsIdentity(
                    new[] {
                new Claim(ClaimTypes.NameIdentifier, acc.Id.ToString()),
                new Claim(ClaimTypes.Name, acc.Name)
                    }, "MyAuthenticationSchema");

                var claimsPrincipal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync("MyAuthenticationSchema", claimsPrincipal);

                // Check if there are wishlist items in the session to transfer to the database
                var wishlist = GetWishlistFromSessionOrCookie(); // Fetch from session or cookies

                // Only proceed if there are items in the wishlist
                if (wishlist != null && wishlist.Any())
                {
                    // Fetch existing wishlist items from the database
                    var existingWishlistItems = await _context.Wishlists
                        .Where(w => w.UserId == acc.Id)
                        .Select(w => w.ProductId)
                        .ToListAsync();

                    // Add only those items from the session that do not already exist in the database
                    foreach (var productId in wishlist)
                    {
                        if (!existingWishlistItems.Contains(productId))
                        {
                            var wishlistItem = new Wishlist
                            {
                                UserId = acc.Id,
                                ProductId = productId
                            };
                            _context.Wishlists.Add(wishlistItem);
                        }
                    }

                    // Save changes to the database
                    await _context.SaveChangesAsync();

                    // Clear the wishlist from session after saving to the database
                    ClearWishlistFromSessionOrCookie();
                }

                return RedirectToAction("Index", "Home");
            }

            return View(model);
        }

        public IActionResult Logout()
        {
            HttpContext.SignOutAsync("MyAuthenticationSchema");
            return RedirectToAction("Index", "Home");
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                // Check if password and confirm password match
                if (model.Password != model.ConfirmPassword)
                {
                    ModelState.AddModelError("PasswordMismatch", "Passwords do not match.");
                    return View(model);
                }

                // Create new User object from RegisterModel
                var user = new User
                {
                    Name = model.Name,
                    Email = model.Email,
                    Password = BCrypt.Net.BCrypt.HashPassword(model.Password), // You might want to hash the password before saving
                    Phone = model.Phone,
                    Address = model.Address
                };

                // Save to database
                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Redirect to login page or any other page
                return RedirectToAction("Login");
            }

            // If there are validation errors, redisplay the form
            return View(model);
        }

        public IActionResult Contact()
        {
            return View();
        }


        public IActionResult Confirmation()
        {
            return View();
        }

        public async Task<IActionResult> Blog(int page = 1, string search = "")
        {
            var pageLimit = 1;

            IQueryable<Blog> query = _context.Blogs;

    
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(b => b.Title.Contains(search));
            }

            query = query.OrderBy(b => b.Id);

            var pagedBlogs =  query.ToPagedList(page, pageLimit);

            ViewBag.Blogs = pagedBlogs; 
            ViewBag.Search=search;
            return View(pagedBlogs); 
        }


        public async Task<IActionResult> BlogDetail(int blogId)
        {
            var query = await _context.Blogs.FirstOrDefaultAsync(b=>b.Id==blogId);
            ViewBag.BlogDetail = query;
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }



        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult UserDetail(int userId)
        {
           var user = _context.Users.Include(u=> u.Orders).SingleOrDefault(u => u.Id == userId);
            ViewBag.User=user;
            return View();
        }

		public IActionResult UserOrderDetail(int orderId, int userId)
		{
            var user = _context.Users.Include(u => u.Orders).SingleOrDefault(u => u.Id == userId);
            ViewBag.User = user;
			var orderdetail =  _context.Orders
				 .Include(o => o.OrderDetails)
				 .ThenInclude(od => od.Product)
				 .FirstOrDefault(m => m.Id == orderId);
			ViewBag.OrderDetail=orderdetail;
			return View();
		}

		public async Task<IActionResult> CancelOrder(int orderId, int userId)
		{

			Order order = await _context.Orders.SingleOrDefaultAsync(o => o.Id == orderId);

			if (order == null)
			{
				return NotFound();
			}
			order.Status = 5; 
			_context.Orders.Update(order);

			await _context.SaveChangesAsync();

			return RedirectToAction("UserOrderDetail", new { orderId=orderId , userId = userId });
		}
		public async Task<IActionResult> ChangePassword(int userId)
		{
            ViewBag.UserId=userId;
			return View();
		}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _context.Users.SingleOrDefaultAsync(u => u.Id == model.UserId);
                if (user == null)
                {
                    return NotFound();
                }

                if (!BCrypt.Net.BCrypt.Verify(model.PasswordOld, user.Password))
                {
                    ModelState.AddModelError("PasswordOld", "The old password is incorrect.");
                    return View(model);
                }

                user.Password = BCrypt.Net.BCrypt.HashPassword(model.Password);
                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                TempData["Message"] = "Password has been successfully changed!";
                await HttpContext.SignOutAsync("MyAuthenticationSchema");
                return RedirectToAction("Login");
            }


            return View(model);
        }

        public async Task<IActionResult> UpdateAccount(int userId)
        {
            var user = _context.Users.Include(u => u.Orders).SingleOrDefault(u => u.Id == userId);
            ViewBag.User = user;
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateAccount(int userId,UserViewModel model, IFormFile? Photo)
        {
            if (ModelState.IsValid)
            {
                var user = await _context.Users.FindAsync(userId); 

                if (user == null)
                {
                    return NotFound();
                }

                user.Name = model.Name;
                user.Phone = model.Phone;
                user.Address = model.Address;    
                if (Photo != null && Photo.Length > 0)
                    {
                        var singleImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", Photo.FileName);
                        using (var stream = new FileStream(singleImagePath, FileMode.Create))
                        {
                            await Photo.CopyToAsync(stream);
                        }
                        user.Avatar = "/images/" + Photo.FileName;
                 }
          
                // Update the user in the database
                _context.Users.Update(user);
                await _context.SaveChangesAsync();
         
                return RedirectToAction("UserDetail", new { userId=userId }); 
            }

            return View(model);
        }

        private List<int> GetWishlistFromSessionOrCookie()
        {
            var wishlist = new List<int>();
            var wishlistSession = HttpContext.Session.GetString("Wishlist");

            if (!string.IsNullOrEmpty(wishlistSession))
            {
                wishlist = JsonConvert.DeserializeObject<List<int>>(wishlistSession);
            }

            return wishlist;
        }


        // Method to clear the wishlist from session or cookies
        private void ClearWishlistFromSessionOrCookie()
        {
            HttpContext.Session.Remove("Wishlist"); // Remove wishlist from session
                                                    // Optionally, clear from cookies if you're using them as well
        }

        [HttpPost]
        [Authorize] // Ensure only logged-in users can post reviews
        public async Task<IActionResult> AddReview(int productId, float ratingStar, string comment)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value); // Get the logged-in user ID

            // Check if the comment is empty
            if (string.IsNullOrWhiteSpace(comment))
            {
                TempData["ErrorMessage"] = "Comment cannot be empty."; // Store error message in TempData
                return RedirectToAction("DetailProduct", new { proId = productId });
            }

            // Check if the user has already reviewed the product
            var existingReview = await _context.Ratings
                .FirstOrDefaultAsync(r => r.UserId == userId && r.ProductId == productId);

            if (existingReview != null)
            {
                // Update existing review
                existingReview.RatingStar = ratingStar;
                existingReview.Comment = comment;
            }
            else
            {
                var review = new Rating
                {
                    ProductId = productId,
                    UserId = userId,
                    RatingStar = ratingStar,
                    Comment = comment
                };
                _context.Ratings.Add(review);
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Review submitted successfully."; // Store success message in TempData
            return RedirectToAction("DetailProduct", new { proId = productId });
        }


    }
}
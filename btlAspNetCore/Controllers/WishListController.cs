using btlAspNetCore.Models.DataModels;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Security.Claims;

namespace btlAspNetCore.Controllers
{
	public class WishListController : Controller
	{
        private readonly AppDbContext _context;

        public WishListController(AppDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                // Fetch wishlist items from the database for logged-in users
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var wishlistItems = _context.Wishlists
                    .Where(w => w.UserId == userId)
                    .Select(w => w.Product)
                    .ToList();

                return View(wishlistItems);
            }
            else
            {
                // Fetch wishlist items from session/cookies for non-logged-in users
                var wishlist = GetWishlistFromSessionOrCookie();
                var wishlistItems = _context.Products
                    .Where(p => wishlist.Contains(p.Id))
                    .ToList();

                return View(wishlistItems);
            }
        }

            [HttpPost]
        public IActionResult AddToWishlist(int productId)
        {
            if (User.Identity.IsAuthenticated)
            {
                // User is logged in, save to database
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var existingWishlistItem = _context.Wishlists.FirstOrDefault(w => w.UserId == userId && w.ProductId == productId);

                if (existingWishlistItem == null)
                {
                    var wishlistItem = new Wishlist
                    {
                        UserId = userId,
                        ProductId = productId
                    };
                    _context.Wishlists.Add(wishlistItem);
                    _context.SaveChanges();
                    TempData["ToastMessage"] = "Product added to wishlist!";
                    TempData["ToastType"] = "success";
                }
                else
                {
                    // Set warning message if product is already in the wishlist
                    TempData["ToastMessage"] = "Product is already in your wishlist!";
                    TempData["ToastType"] = "warning";
                }
            }
            else
            {
                // User not logged in, save to session/cookies
                var wishlist = GetWishlistFromSessionOrCookie();

                if (!wishlist.Contains(productId))
                {
                    wishlist.Add(productId);
                    SaveWishlistToSessionOrCookie(wishlist);
                    TempData["ToastMessage"] = "Product added to wishlist!";
                    TempData["ToastType"] = "success";
                }
           
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult RemoveFromWishlist(int productId)
        {
            if (User.Identity.IsAuthenticated)
            {
                // User is logged in, remove from database
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var wishlistItem = _context.Wishlists.FirstOrDefault(w => w.UserId == userId && w.ProductId == productId);

                if (wishlistItem != null)
                {
                    _context.Wishlists.Remove(wishlistItem);
                    _context.SaveChanges();
                    TempData["ToastMessage"] = "Product removed from wishlist!";
                    TempData["ToastType"] = "success";
                }
                else
                {
                    // Set error message if product was not in the wishlist
                    TempData["ToastMessage"] = "Product not found in your wishlist!";
                    TempData["ToastType"] = "error";
                }
            }
            else
            {
                // User not logged in, remove from session/cookies
                var wishlist = GetWishlistFromSessionOrCookie();

                if (wishlist.Contains(productId))
                {
                    wishlist.Remove(productId);
                    SaveWishlistToSessionOrCookie(wishlist);
                    TempData["ToastMessage"] = "Product removed from wishlist!";
                    TempData["ToastType"] = "success";
                }
            }
            return RedirectToAction("Index");
        }


        private List<int> GetWishlistFromSessionOrCookie()
        {
            // Retrieve wishlist from session or cookies (similar to the previous implementation)
            var wishlist = new List<int>();
            var wishlistSession = HttpContext.Session.GetString("Wishlist");
            if (!string.IsNullOrEmpty(wishlistSession))
            {
                wishlist = JsonConvert.DeserializeObject<List<int>>(wishlistSession);
            }
            return wishlist;
        }


        private void SaveWishlistToSessionOrCookie(List<int> wishlist)
        {
            // Save wishlist to session or cookies
            HttpContext.Session.SetString("Wishlist", JsonConvert.SerializeObject(wishlist));
        }
    }
}

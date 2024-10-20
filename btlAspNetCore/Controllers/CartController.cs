using btlAspNetCore.Models.DataModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace btlAspNetCore.Controllers
{
    public class CartController : Controller
    {
        private readonly AppDbContext _context;

        public CartController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var cart = GetCartItems();
            return View(cart);
        }

        [HttpPost]
        public IActionResult AddToCart(int productId, string productName, float price, int quantity = 1, string image="")
        {
        
            var cart = GetCartItems();

            var cartItem = cart.FirstOrDefault(c => c.ProductId == productId);

            if (cartItem == null)
            {
                cart.Add(new CartItem
                {
                    ProductId = productId,
                    ProductName = productName,
                    Price = price,
                    Quantity = quantity, 
                    Image = image
                });
            }
            else
            {
                cartItem.Quantity += quantity;
            }

            SaveCartSession(cart);

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult UpdateCart(int productId, int quantity)
        {
            var cart = GetCartItems();

            var cartItem = cart.FirstOrDefault(c => c.ProductId == productId);

            if (cartItem != null)
            {
                if (quantity > 0)
                {
                    cartItem.Quantity = quantity;
                }
                else
                {
                    cart.Remove(cartItem);
                }

                SaveCartSession(cart);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult RemoveFromCart(int productId)
        {
            var cart = GetCartItems();
            var cartItem = cart.FirstOrDefault(c => c.ProductId == productId);
            if (cartItem != null)
            {
                cart.Remove(cartItem);
            }

            SaveCartSession(cart);
            return RedirectToAction(nameof(Index));
        }

        public IActionResult ClearCart()
        {
            SaveCartSession(new List<CartItem>());
            return RedirectToAction(nameof(Index));
        }

        [Authorize]
        [HttpGet]
        public IActionResult CheckOut()
        {
            var cart = GetCartItems();
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            ViewBag.User = _context.Users.FirstOrDefault(u => u.Id == int.Parse(userId));

            return View(cart);
        }

        [HttpPost]
        public async Task<IActionResult> CheckOut(string name, string phone, string email, string address, string? orderNote,byte paymentMethod)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            var currentUser = _context.Users.FirstOrDefault(u => u.Id == int.Parse(userId));
            if (currentUser == null)
            {
                
                return NotFound();
            }
            bool userUpdated = false;
            if (currentUser.Name != name)
            {
                currentUser.Name = name;
                userUpdated = true;
            }

            if (currentUser.Phone != phone)
            {
                currentUser.Phone = phone;
                userUpdated = true;
            }

            if (currentUser.Email != email)
            {
                currentUser.Email = email;
                userUpdated = true;
            }

            if (currentUser.Address != address)
            {
                currentUser.Address = address;
                userUpdated = true;
            }

            if (userUpdated)
            {
                _context.Users.Update(currentUser);
                _context.SaveChanges();
            }

            // Get cart items
            var cartItems = GetCartItems();
            if (cartItems == null || !cartItems.Any())
            {
                // Handle empty cart case
                return RedirectToAction("Index", "Cart"); 
            }


            var order = new Order
            {
                UserId = int.Parse(userId),
                OrderNote = orderNote,
                Status = 1, // Example status
                PaymentMethod = paymentMethod,
                OrderDetails = new List<OrderDetail>() 
            };

            // Save the order first
           await  _context.Orders.AddAsync(order);
            await  _context.SaveChangesAsync(); 

            // Add order details
            foreach (var item in cartItems)
            {
                if (item != null) 
                {
                    var orderDetail = new OrderDetail
                    {
                        OrderId = order.Id, 
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        TotalPrice = item.Price * item.Quantity
                    };

                    order.OrderDetails.Add(orderDetail);
                }
            }

            // Save the order details to the database
            _context.SaveChanges();

            ClearCart();
            return RedirectToAction("Confirmation","Home");

        }



        List<CartItem> GetCartItems()
        {
            var session = HttpContext.Session;
            string jsonCart = session.GetString("cart");
            if (jsonCart != null)
            {
                return JsonConvert.DeserializeObject<List<CartItem>>(jsonCart);
            }
            return new List<CartItem>();
        }



        void SaveCartSession(List<CartItem> cart)
        {
            var session = HttpContext.Session;
            string jsonCart = JsonConvert.SerializeObject(cart);
            session.SetString("cart", jsonCart);
        }
    }
}

using btlAspNetCore.Models.DataModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol.Plugins;
using System.Security.Claims;
using System.Security.Principal;

namespace btlAspNetCore.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class LoginController : Controller
    {
        private readonly AppDbContext _context;
        public LoginController(AppDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Index(LoginModel model)
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
                if (acc.Role != 1)
                {
                    ModelState.AddModelError(string.Empty, "You don't have permission");
                    return View(model);
                }

                var identity = new ClaimsIdentity(new[]
                    {
                        new Claim(ClaimTypes.Name, acc.Name),
                        new Claim(ClaimTypes.Email, acc.Email),
                        new Claim(ClaimTypes.Role, acc.Role.ToString())
                    }, "MyAppAuthenticationAdmin");

                    var parincipal = new ClaimsPrincipal(identity);
                    await HttpContext.SignInAsync("MyAppAuthenticationAdmin", parincipal);
                    return RedirectToAction("Index", "Home");             
            }
            return View();
        }

        public IActionResult Logout()
        {
            HttpContext.SignOutAsync("MyAppAuthenticationAdmin");
            return RedirectToAction("Index", "Login");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

    }
}

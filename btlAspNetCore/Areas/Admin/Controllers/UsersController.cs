using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using btlAspNetCore.Models.DataModels;
using X.PagedList.Extensions;
using System.Reflection.Metadata;

namespace btlAspNetCore.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class UsersController : BaseController
    {
        private readonly AppDbContext _context;

        public UsersController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Users
        public async Task<IActionResult> Index(string? searchTerm, int page = 1)
        {
            int pageSize = 5; 

            IQueryable<User> usersQuery = _context.Users.OrderBy(u => u.Id);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                ViewBag.SearchTerm = searchTerm;
                usersQuery = usersQuery.Where(u =>
                    u.Name.Contains(searchTerm) ||
                    u.Email.Contains(searchTerm) ||
                    u.Phone.Contains(searchTerm));
            }
            
            var listUsers =  usersQuery.ToPagedList(page, pageSize);
   
            return View(listUsers);
        }


        // GET: Admin/Users/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Users == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(m => m.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // GET: Admin/Users/Create
        public IActionResult Create()
        {
            ViewBag.RoleOptions = new List<SelectListItem>
            {
                new SelectListItem { Value = "1", Text = "Admin" },
                new SelectListItem { Value = "0", Text = "Customer" },
            };

            return View();
        }

        // POST: Admin/Users/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Email,Password,Avatar,Phone,Address,Role")] User user, IFormFile? singleImage)
        {
            if (ModelState.IsValid)
            {
                if (singleImage != null && singleImage.Length > 0)
                {
                    var singleImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", singleImage.FileName);
                    using (var stream = new FileStream(singleImagePath, FileMode.Create))
                    {
                        await singleImage.CopyToAsync(stream);
                    }
                    user.Avatar = "/images/" + singleImage.FileName;
                }
                user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
                try
                {
                    _context.Add(user);
                    await _context.SaveChangesAsync();
                    TempData["ToastMessage"] = "Customer created successfully!";
                    TempData["ToastType"] = "success";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException ex)
                {
                    if (ex.InnerException != null)
                    {

                        ModelState.AddModelError("Email", "Email must be unique.");
                    }
                }
            }
            ViewBag.RoleOptions = new List<SelectListItem>
            {
                new SelectListItem { Value = "1", Text = "Admin" },
                new SelectListItem { Value = "0", Text = "Customer" },
            };
            TempData["ToastMessage"] = "Customer to create Customer.";
            TempData["ToastType"] = "error";
            return View(user);
        }

        // GET: Admin/Users/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Users == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            ViewBag.RoleOptions = new List<SelectListItem>
            {
                new SelectListItem { Value = "1", Text = "Admin" },
                new SelectListItem { Value = "0", Text = "Customer" },
            };
            return View(user);
        }

        // POST: Admin/Users/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Email,Password,Avatar,Phone,Address,Role")] User user, IFormFile? singleImage)
        {
            if (id != user.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
                if (existingUser == null)
                {
                    return NotFound();
                }

                existingUser.Name = user.Name;
                existingUser.Email = user.Email;
                existingUser.Phone = user.Phone;
                existingUser.Address= user.Address;
                existingUser.Role = user.Role;

                if (!BCrypt.Net.BCrypt.Verify(user.Password, existingUser.Password))
                {
                    existingUser.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
                }

                if (singleImage != null && singleImage.Length > 0)
                {
                    var singleImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", singleImage.FileName);
                    using (var stream = new FileStream(singleImagePath, FileMode.Create))
                    {
                        await singleImage.CopyToAsync(stream);
                    }
                    existingUser.Avatar = "/images/" + singleImage.FileName;
                }
               

                try
                {
                    _context.Update(existingUser);
                    await _context.SaveChangesAsync();
                    TempData["ToastMessage"] = "Customer update successfully!";
                    TempData["ToastType"] = "success";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(user.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (DbUpdateException ex)
                {
                    if (ex.InnerException != null)
                    {

                        ModelState.AddModelError("Email", "Email must be unique.");
                    }
                }
            }
            
            ViewBag.RoleOptions = new List<SelectListItem>
            {
                new SelectListItem { Value = "1", Text = "Admin" },
                new SelectListItem { Value = "0", Text = "Customer" },
            };
            TempData["ToastMessage"] = "Failed to Update Customer.";
            TempData["ToastType"] = "error";
            return View(user);
        }

        // GET: Admin/Users/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Users == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(m => m.Id == id);
            if (user == null)
            {
                return NotFound();
            }
            if (user != null)
            {
                _context.Users.Remove(user);
                TempData["ToastMessage"] = "Customer deleted successfully!";
                TempData["ToastType"] = "success";
            }

            await _context.SaveChangesAsync();
            TempData["ToastMessage"] = "Failed to delted Customer.";
            TempData["ToastType"] = "error";
            return RedirectToAction(nameof(Index));
        }

        private bool UserExists(int id)
        {
          return (_context.Users?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}

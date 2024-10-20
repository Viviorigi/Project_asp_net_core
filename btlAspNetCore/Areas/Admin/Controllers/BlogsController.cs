using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using btlAspNetCore.Models.DataModels;
using X.PagedList.Extensions;

namespace btlAspNetCore.Areas.Admin.Controllers
{

    public class BlogsController : BaseController
    {
        private readonly AppDbContext _context;

        public BlogsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Blogs
        public async Task<IActionResult> Index(string? name, int page = 1)
        {
            int limit = 5;
            if (!string.IsNullOrWhiteSpace(name))
            {
                ViewBag.Name = name;
                var query = _context.Blogs
                                    .Where(b => b.Title.Contains(name))
                                    .OrderBy(b => b.Id);
                return _context.Blogs != null ?
                    View(query.ToPagedList(page, limit)) :
                    Problem("Entity set 'AppDbContext.Blog' is null.");
            }
            var allBlogs = _context.Blogs.OrderByDescending(b => b.Id);
            return _context.Blogs != null ?
                          View(allBlogs.ToPagedList(page, limit)) :
                          Problem("Entity set 'AppDbContext.Blog'  is null.");
        }

        // GET: Admin/Blogs/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/Blogs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,BlogContent")] Blog blog, IFormFile singleImage)
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
                    blog.Image = "/images/" + singleImage.FileName; 
                }
                _context.Add(blog);
                await _context.SaveChangesAsync();
                TempData["ToastMessage"] = "Blog created successfully!";
                TempData["ToastType"] = "success";
                return RedirectToAction(nameof(Index));
            }
            //ViewBag.Error= ModelState.Values.SelectMany(v => v.Errors)
            //                    .Select(e => e.ErrorMessage)
            //                    .ToList();
            TempData["ToastMessage"] = "Failed to create Blog.";
            TempData["ToastType"] = "error";
            return View(blog);
        }

        // GET: Admin/Blogs/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Blogs == null)
            {
                return NotFound();
            }

            var blog = await _context.Blogs.FindAsync(id);
            if (blog == null)
            {
                return NotFound();
            }
            return View(blog);
        }

        // POST: Admin/Blogs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Image,Title,BlogContent")] Blog blog, IFormFile? singleImage)
        {
            if (id != blog.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingBlog = await _context.Blogs.FirstOrDefaultAsync(b => b.Id == id);
                    if (existingBlog == null)
                    {
                        return NotFound();
                    }
                    // Update product properties
                    existingBlog.Title = blog.Title;
                    existingBlog.BlogContent = blog.BlogContent;

                    // Check if a new single image is uploaded
                    if (singleImage != null && singleImage.Length > 0)
                    {
                        // Handle the single image upload here (e.g., save to a directory and set the path)
                        var singleImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", singleImage.FileName);
                        using (var stream = new FileStream(singleImagePath, FileMode.Create))
                        {
                            await singleImage.CopyToAsync(stream);
                        }
                        // Set the Img property to the new image path
                        existingBlog.Image = "/images/" + singleImage.FileName; // Update the image path in the existing product
                    }

                    _context.Update(existingBlog);
                    await _context.SaveChangesAsync();
                    TempData["ToastMessage"] = "Blog Update successfully!";
                    TempData["ToastType"] = "success";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BlogExists(blog.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                TempData["ToastMessage"] = "Failed to Update blog.";
                TempData["ToastType"] = "error";
                return RedirectToAction(nameof(Index));
            }
            return View(blog);
        }

        // GET: Admin/Blogs/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Blogs == null)
            {
                return NotFound();
            }

            var blog = await _context.Blogs
                .FirstOrDefaultAsync(m => m.Id == id);
            if (blog == null)
            {
                return NotFound();
            }
            if (blog != null)
            {
                _context.Blogs.Remove(blog);
                TempData["ToastMessage"] = "Blog removed successfully!";
                TempData["ToastType"] = "success";
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }



        private bool BlogExists(int id)
        {
          return (_context.Blogs?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}

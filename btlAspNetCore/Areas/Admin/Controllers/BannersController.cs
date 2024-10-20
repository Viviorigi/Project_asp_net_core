using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using btlAspNetCore.Models.DataModels;
using System.Reflection.Metadata;
using X.PagedList.Extensions;

namespace btlAspNetCore.Areas.Admin.Controllers
{

    public class BannersController : BaseController
    {
        private readonly AppDbContext _context;

        public BannersController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Banners
        public async Task<IActionResult> Index(string? name, int page = 1)
        {
            int limit = 5;
            if (!string.IsNullOrWhiteSpace(name))
            {
                ViewBag.Name = name;
                var query = _context.Banners
                                    .Where(b => b.Title.Contains(name))
                                    .OrderBy(b => b.Id);
                return _context.Banners != null ?
                    View(query.ToPagedList(page, limit)) :
                    Problem("Entity set 'AppDbContext.Banner' is null.");
            }
            var allBanners = _context.Banners.OrderByDescending(b => b.Id);
            return _context.Banner != null ? 
                          View(allBanners.ToPagedList(page, limit)) :
                          Problem("Entity set 'AppDbContext.Banner'  is null.");
        }

        // GET: Admin/Banners/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/Banners/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,shortDescription")] Banner banner, IFormFile singleImage)
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
                    banner.Image = "/images/" + singleImage.FileName;
                }
                _context.Add(banner);
                await _context.SaveChangesAsync();
                TempData["ToastMessage"] = "Banner created successfully!";
                TempData["ToastType"] = "success";
                return RedirectToAction(nameof(Index));
            }
            TempData["ToastMessage"] = "Failed to create Banner.";
            TempData["ToastType"] = "error";
            return View(banner);
        }

        // GET: Admin/Banners/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Banner == null)
            {
                return NotFound();
            }

            var banner = await _context.Banner.FindAsync(id);
            if (banner == null)
            {
                return NotFound();
            }
            return View(banner);
        }

        // POST: Admin/Banners/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,shortDescription")] Banner banner, IFormFile singleImage)
        {
            if (id != banner.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                        var existingBanner = await _context.Banners.FirstOrDefaultAsync(b => b.Id == id);
                        if (existingBanner == null)
                        {
                            return NotFound();
                        }
                        // Update product properties
                        existingBanner.Title = banner.Title;
                       

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
                            existingBanner.Image = "/images/" + singleImage.FileName; // Update the image path in the existing product
                        }

                        _context.Update(existingBanner);
                    await _context.SaveChangesAsync();
                    TempData["ToastMessage"] = "Banner Update successfully!";
                    TempData["ToastType"] = "success";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BannerExists(banner.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            TempData["ToastMessage"] = "Failed to Update Banner.";
            TempData["ToastType"] = "error";
            return View(banner);
        }

        // GET: Admin/Banners/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Banner == null)
            {
                return NotFound();
            }

            var banner = await _context.Banner
                .FirstOrDefaultAsync(m => m.Id == id);
            if (banner == null)
            {
                return NotFound();
            }
            if (banner != null)
            {
                _context.Banner.Remove(banner);
            }

            await _context.SaveChangesAsync();
            TempData["ToastMessage"] = "Banner removed successfully!";
            TempData["ToastType"] = "success";
            return RedirectToAction(nameof(Index));

        }

        private bool BannerExists(int id)
        {
          return (_context.Banner?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}

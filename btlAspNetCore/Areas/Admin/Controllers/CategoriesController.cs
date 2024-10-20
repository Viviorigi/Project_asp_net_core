using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using btlAspNetCore.Models.DataModels;
using X.PagedList.Extensions;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace btlAspNetCore.Areas.Admin.Controllers
{
  
    public class CategoriesController : BaseController
    {
        private readonly AppDbContext _context;

        public CategoriesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Categories
        public async Task<IActionResult> Index(string? name, int page = 1)
        {
            int limit = 5;

            // Query to get the categories along with the product count
            var query = _context.Categories
                                .Select(c => new CategoryViewModel
                                {
                                    Category = c,
                                    ProductCount = c.Products.Count() // Assuming 'Products' is the navigation property
                                });

            // Apply filtering if name is provided
            if (!string.IsNullOrWhiteSpace(name))
            {
                ViewBag.Name = name;
                query = query.Where(c => c.Category.Name.Contains(name));
            }

            // Apply ordering after filtering
            var orderedQuery = query.OrderByDescending(c => c.Category.Id);

            // Using X.PagedList to paginate the results
            var pagedResult = orderedQuery.ToPagedList(page, limit);

            return _context.Categories != null ?
                View(pagedResult) :
                Problem("Entity set 'AppDbContext.Categories' is null.");
        }

        // GET: Admin/Categories/Create
        public IActionResult Create()
        {
            ViewBag.StatusOptions = new List<SelectListItem>
            {
                new SelectListItem { Value = "0", Text = "Inactive" },
                new SelectListItem { Value = "1", Text = "Active" }
            };

            return View();
        }

        // POST: Admin/Categories/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Status")] Category category)
        {
            ViewBag.StatusOptions = new List<SelectListItem>
            {
                new SelectListItem { Value = "0", Text = "Inactive" },
                new SelectListItem { Value = "1", Text = "Active" }
            };

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(category);
                    await _context.SaveChangesAsync();
                    TempData["ToastMessage"] = "Category created successfully!";
                    TempData["ToastType"] = "success";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException ex)
                {
                    if (ex.InnerException != null)
                    {
                 
                        ModelState.AddModelError("Name", "Category name must be unique.");
                    }
                }
            }
            TempData["ToastMessage"] = "Failed to create category.";
            TempData["ToastType"] = "error";

            return View(category);
        }


        // GET: Admin/Categories/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Categories == null)
            {
                return NotFound();
            }

            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            ViewBag.StatusOptions = new List<SelectListItem>
            {
                new SelectListItem { Value = "0", Text = "Inactive" },
                new SelectListItem { Value = "1", Text = "Active" }
            };
            return View(category);
        }

        // POST: Admin/Categories/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Status")] Category category)
        {
            if (id != category.Id)
            {
                return NotFound();
            }
            ViewBag.StatusOptions = new List<SelectListItem>
            {
                new SelectListItem { Value = "0", Text = "Inactive" },
                new SelectListItem { Value = "1", Text = "Active" }
            };

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(category);
                    await _context.SaveChangesAsync();
                    TempData["ToastMessage"] = "Category updated successfully!";
                    TempData["ToastType"] = "success";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(category.Id))
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
                    if (ex.InnerException != null )
                    {

                        ModelState.AddModelError("Name", "Category name must be unique.");
                    }
                }           
            }
            TempData["ToastMessage"] = "Failed to update category.";
            TempData["ToastType"] = "error";
            return View(category);
        }

        // GET: Admin/Categories/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Categories == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .Include(c => c.Products) // Include the related products to check if any exist
                .FirstOrDefaultAsync(m => m.Id == id);

            if (category == null)
            {
                return NotFound();
            }

            // Check if there are any products associated with this category
            if (category.Products.Any())
            {
                // Add an error message if there are products linked to this category
                TempData["ErrorMessage"] = "Cannot delete the category because there are products associated with it. Please delete all associated products first.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
                TempData["ToastMessage"] = "Category deleted successfully!";
                TempData["ToastType"] = "success";
            }
            catch (DbUpdateException ex)
            {
                TempData["ToastMessage"] = "Category not found!";
                TempData["ToastType"] = "error";
            }

            return RedirectToAction(nameof(Index));
        }


        private bool CategoryExists(int id)
        {
          return (_context.Categories?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}

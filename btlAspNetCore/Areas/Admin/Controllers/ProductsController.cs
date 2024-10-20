using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using btlAspNetCore.Models.DataModels;
using static System.Net.Mime.MediaTypeNames;
using X.PagedList.Extensions;
using btlAspNetCore.Areas.Admin.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace btlAspNetCore.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductsController : BaseController
    {
        private readonly AppDbContext _context;

        public ProductsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Products
        public async Task<IActionResult> Index(string? name, int page = 1,int cateId=0)
        {
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name");
  
            int limit = 5;
            if (!string.IsNullOrWhiteSpace(name) && cateId==0)
            {
                ViewBag.Name = name;
                var query = _context.Products.Include(p => p.Category)
                                    .Where(p => p.Name.Contains(name))
                                    .OrderBy(p => p.Id);
                var viewModel = new ProductViewModel
                {
                    Products = query.ToPagedList(page, limit),
                    CateId = cateId,
                    Name = name
                };
                return _context.Products != null ?
                    View(viewModel) :
                    Problem("Entity set 'AppDbContext.Banner' is null.");
            }else if (!string.IsNullOrWhiteSpace(name) && cateId > 0)
            {
                ViewBag.Name = name;
                var query = _context.Products.Include(p => p.Category)
                                    .Where(p => p.Name.Contains(name)).Where(p => p.CategoryId == cateId)
                                    .OrderBy(p => p.Id);
                var viewModel = new ProductViewModel
                {
                    Products = query.ToPagedList(page, limit),
                    CateId = cateId,
                    Name = name
                };
                return _context.Products != null ?
                    View(viewModel) :
                    Problem("Entity set 'AppDbContext.Banner' is null.");
            }
            if (cateId > 0)
            {
                var data = _context.Products.Include(p => p.Category).Where(p => p.CategoryId==cateId).OrderBy(p => p.Id);
                var viewdata = new ProductViewModel
                {
                    Products = data.ToPagedList(page, limit),
                    CateId = cateId,
                    Name = name
                };
                return _context.Products != null ? View(viewdata) :
              Problem("Entity set 'AppDbContext.Product' is null.");
            }
            var appDbContext = _context.Products.Include(p => p.Category).OrderBy(p => p.Id);
            var view = new ProductViewModel
            {
                Products = appDbContext.ToPagedList(page, limit),
                CateId = cateId,
                Name = name
            };
            return _context.Products != null ? View(view) :
                Problem("Entity set 'AppDbContext.Product' is null.");
        }

        // GET: Admin/Products/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Products == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // GET: Admin/Products/Create
        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name");
            ViewBag.StatusOptions = new List<SelectListItem>
            {
                new SelectListItem { Value = "0", Text = "Inactive" },
                new SelectListItem { Value = "1", Text = "Active" }
            };
            return View();
        }

        // POST: Admin/Products/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Price,SalePrice,Status,Description,CategoryId")] Product product, IFormFile? singleImage, List<IFormFile> multipleImages)
        {
            if (ModelState.IsValid)
            {
                // Set CreatedAt to current time
                product.CreatedAt = DateTime.Now;

                // Handle single image upload
                if (singleImage != null && singleImage.Length > 0)
                {
                    // Specify the path where the image will be saved
                    var singleImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", singleImage.FileName);
                    using (var stream = new FileStream(singleImagePath, FileMode.Create))
                    {
                        await singleImage.CopyToAsync(stream);
                    }
                    product.Img = "/images/" + singleImage.FileName; // Save the image file name to the Product entity
                }

                // Handle multiple images upload
                if (multipleImages != null && multipleImages.Count > 0)
                {
                    foreach (var image in multipleImages)
                    {
                        if (image.Length > 0)
                        {
                            var multipleImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", image.FileName);
                            using (var stream = new FileStream(multipleImagePath, FileMode.Create))
                            {
                                await image.CopyToAsync(stream);
                            }

                            // Create ImgProduct entry for each uploaded image
                            var imgProduct = new ImgProduct
                            {
                                Image = "/images/" + image.FileName, // Store the image path or filename
                                Product = product // Associate the image with the product
                            };
                            product.ImgProducts.Add(imgProduct); // Add to the product's image collection
                        }
                    }
                }
                try
                {
                    _context.Add(product);
                    await _context.SaveChangesAsync();
                    TempData["ToastMessage"] = "Product created successfully!";
                    TempData["ToastType"] = "success";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException ex)
                {
                    if (ex.InnerException != null)
                    {

                        ModelState.AddModelError("Name", "Product name must be unique.");
                    }
                }
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
            ViewBag.StatusOptions = new List<SelectListItem>
            {
                new SelectListItem { Value = "0", Text = "Inactive" },
                new SelectListItem { Value = "1", Text = "Active" }
            };
            TempData["ToastMessage"] = "Failed to create product.";
            TempData["ToastType"] = "error";
            return View(product);
        }

        // GET: Admin/Products/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Products == null)
            {
                return NotFound();
            }
            var product = _context.Products.Include(p => p.ImgProducts)
                                              .FirstOrDefault(p => p.Id == id);
            if (product == null)
            {
                return NotFound();
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
            ViewBag.StatusOptions = new List<SelectListItem>
            {
                new SelectListItem { Value = "0", Text = "Inactive" },
                new SelectListItem { Value = "1", Text = "Active" }
            };
            return View(product);
        }

        // POST: Admin/Products/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Price,SalePrice,Status,Description,CategoryId")] Product product, IFormFile? singleImage, List<IFormFile> multipleImages)
        {
            if (id != product.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Find the existing product in the database
                    var existingProduct = await _context.Products.Include(p => p.ImgProducts).FirstOrDefaultAsync(p => p.Id == id);
                    if (existingProduct == null)
                    {
                        return NotFound();
                    }

                    // Update product properties
                    existingProduct.Name = product.Name;
                    existingProduct.Price = product.Price;
                    existingProduct.SalePrice = product.SalePrice;
                    existingProduct.Status = product.Status;
                    existingProduct.Description = product.Description;
                    existingProduct.CategoryId = product.CategoryId;

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
                        existingProduct.Img = "/images/" + singleImage.FileName; // Update the image path in the existing product
                    }
                    // Handle multiple image uploads
                    if (multipleImages != null && multipleImages.Count > 0)
                    {
                        // Clear existing images if needed or keep them
                        existingProduct.ImgProducts.Clear(); 

                        foreach (var image in multipleImages)
                        {
                            if (image.Length > 0)
                            {
                                var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", image.FileName);
                                using (var stream = new FileStream(imagePath, FileMode.Create))
                                {
                                    await image.CopyToAsync(stream);
                                }
                                // Add new image record
                                existingProduct.ImgProducts.Add(new ImgProduct { Image = "/images/" + image.FileName, ProductId = existingProduct.Id });
                            }
                        }
                    }

                    _context.Update(existingProduct);
                    await _context.SaveChangesAsync();
                    TempData["ToastMessage"] = "Product updated successfully!";
                    TempData["ToastType"] = "success";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.Id))
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

                        ModelState.AddModelError("Name", "Product name must be unique.");
                    }
                }
            }
            // Prepare ViewData for the view
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
            ViewBag.StatusOptions = new List<SelectListItem>
            {
                new SelectListItem { Value = "0", Text = "Inactive" },
                new SelectListItem { Value = "1", Text = "Active" }
            };
            //ViewBag.Errors = ModelState.Values.SelectMany(v => v.Errors)
            //                          .Select(e => e.ErrorMessage)
            //                          .ToList();
            TempData["ToastMessage"] = "Failed to update product.";
            TempData["ToastType"] = "error";
            return View(product);
        }


        // GET: Admin/Products/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Products == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product != null)
            {
                _context.Products.Remove(product);
            }

            await _context.SaveChangesAsync();
            if (product == null)
            {
                return NotFound();
            }
            TempData["ToastMessage"] = "Product deleted successfully!";
            TempData["ToastType"] = "success";
            return RedirectToAction(nameof(Index));
        }


        private bool ProductExists(int id)
        {
          return (_context.Products?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shop.Data;
using Shop.Models.ViewModels;
using Shop.Utility;

namespace Shop.Areas.Admin.Controllers
{
    [Authorize(Roles = StaticDefault.ManagerUser)]
    [Area("Admin")]
    public class ProductController : Controller
    {

        private readonly ApplicationDbContext _db;
        private readonly IHostingEnvironment _hostingEnvironment;

        [BindProperty]
        public ProductViewModel ProductVM { get; set; }

        public ProductController(ApplicationDbContext db, IHostingEnvironment hostingEnvironment)
        {
            _db = db;
            _hostingEnvironment = hostingEnvironment;
            ProductVM = new ProductViewModel()
            {
                Category = _db.Category,
                Product = new Models.Product()
        };

        }

        public async Task<IActionResult> Index()
        {
            var Prodcuts = await _db.Product.Include(x=>x.Category).Include(x=>x.SubCategory).ToListAsync();
            return View(Prodcuts);
        }

        public IActionResult Create()
        {
            return View(ProductVM);
        }

        [HttpPost,ActionName("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePost()
        {
            ProductVM.Product.SubCategoryId = Convert.ToInt32(Request.Form["SubCategoryId"].ToString());
            if (!ModelState.IsValid)
            {
                return View(ProductVM);
            }
                _db.Product.Add(ProductVM.Product);
            await _db.SaveChangesAsync();

            string webRootPath = _hostingEnvironment.WebRootPath;
            var files = HttpContext.Request.Form.Files;

            var ProductDb = await _db.Product.FindAsync(ProductVM.Product.Id);

            if(files.Count>0)
            {
                var uploads = Path.Combine(webRootPath, "images");
                var extension = Path.GetExtension(files[0].FileName);

                using (var fileStream = new FileStream(Path.Combine(uploads, ProductVM.Product.Id + extension), FileMode.Create))
                {
                    files[0].CopyTo(fileStream);
                }
                ProductDb.Image = @"\images\" + ProductVM.Product.Id + extension;
            }
            else
            {
                var uploads = Path.Combine(webRootPath, @"images\"+StaticDefault.DefaultProductImage);
                System.IO.File.Copy(uploads, webRootPath + @"\images\" + ProductVM.Product.Id + ".jpg");
                ProductDb.Image = @"\images\" + ProductVM.Product.Id + ".jpg";
            }

            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            ProductVM.Product = await _db.Product.Include(x => x.Category).Include(X => X.SubCategory).SingleOrDefaultAsync(x => x.Id == id);
            ProductVM.SubCategory = await _db.SubCategory.Where(x => x.CategoryId == ProductVM.Product.CategoryId).ToListAsync();

            if (ProductVM.Product == null)
            {
                return NotFound();
            }
                return View(ProductVM);
        }

        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPost(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            ProductVM.Product.SubCategoryId = Convert.ToInt32(Request.Form["SubCategoryId"].ToString());
            if (!ModelState.IsValid)
            {
                ProductVM.SubCategory = await _db.SubCategory.Where(x => x.CategoryId == ProductVM.Product.CategoryId).ToListAsync();
                return View(ProductVM);
            }


            string webRootPath = _hostingEnvironment.WebRootPath;
            var files = HttpContext.Request.Form.Files;

            var ProductDb = await _db.Product.FindAsync(ProductVM.Product.Id);

            if (files.Count > 0)
            {
                var uploads = Path.Combine(webRootPath, "images");
                var new_extension = Path.GetExtension(files[0].FileName);

                var imagePath = Path.Combine(webRootPath, ProductDb.Image.TrimStart('\\'));

                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }

                using (var fileStream = new FileStream(Path.Combine(uploads, ProductVM.Product.Id + new_extension), FileMode.Create))
                {
                    files[0].CopyTo(fileStream);
                }
                ProductDb.Image = @"\images\" + ProductVM.Product.Id + new_extension;
            }

            ProductDb.Name = ProductVM.Product.Name;
            ProductDb.Description = ProductVM.Product.Description;
            ProductDb.Country = ProductVM.Product.Country;
            ProductDb.Price = ProductVM.Product.Price;
            ProductDb.SubCategoryId = ProductVM.Product.SubCategoryId;
            ProductDb.CategoryId = ProductVM.Product.CategoryId;
            ProductDb.ShortDescription = ProductVM.Product.ShortDescription;
            ProductDb.Weight = ProductVM.Product.Weight;
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            ProductVM.Product = await _db.Product.Include(x => x.Category).Include(X => X.SubCategory).SingleOrDefaultAsync(x => x.Id == id);
            ProductVM.SubCategory = await _db.SubCategory.Where(x => x.CategoryId == ProductVM.Product.CategoryId).ToListAsync();

            if (ProductVM.Product == null)
            {
                return NotFound();
            }
            return View(ProductVM);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            ProductVM.Product = await _db.Product.Include(x => x.Category).Include(X => X.SubCategory).SingleOrDefaultAsync(x => x.Id == id);
            ProductVM.SubCategory = await _db.SubCategory.Where(x => x.CategoryId == ProductVM.Product.CategoryId).ToListAsync();

            if (ProductVM.Product == null)
            {
                return NotFound();
            }
            return View(ProductVM);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAction(int? ID)
        {
            var Product = await _db.Product.FindAsync(ID);
            if (Product == null)
            {
                return View(ProductVM);
            }
            string webRootPath = _hostingEnvironment.WebRootPath;
            var imagePath = Path.Combine(webRootPath, Product.Image.TrimStart('\\'));
            System.IO.File.Delete(imagePath);
            _db.Product.Remove(Product);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));

        }
    }
}
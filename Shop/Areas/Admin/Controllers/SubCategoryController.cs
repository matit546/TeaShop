using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Shop.Data;
using Shop.Models;
using Shop.Models.ViewModels;
using Shop.Utility;

namespace Shop.Areas.Admin.Controllers
{
    [Authorize(Roles = StaticDefault.ManagerUser)]
    [Area("Admin")]
    public class SubCategoryController : Controller
    {
        private readonly ApplicationDbContext _db;

        public SubCategoryController(ApplicationDbContext db)
        {
            _db = db;
        }

        [TempData]
        public string MessageError { get; set; }

        public async Task<IActionResult> Index()
        {
            var SubCategory = await _db.SubCategory.Include(x => x.Category).ToListAsync();
            return View(SubCategory);
        }

        public async Task<IActionResult> Create()
        {
            SubCategory_Category_ViewModel model = new SubCategory_Category_ViewModel()
            {
                CategoryList = await _db.Category.ToListAsync(),
                SubCategory = new Models.SubCategory(),
                SubCategoryList = await _db.SubCategory.OrderBy(x => x.Name).Select(x => x.Name).Distinct().ToListAsync()
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SubCategory_Category_ViewModel model)
        {
            if (ModelState.IsValid)
            {
                var Subcategoryexists = _db.SubCategory.Include(x => x.Category).Where(x => x.Name == model.SubCategory.Name && x.Category.Id == model.SubCategory.CategoryId);

                if (Subcategoryexists.Count() > 0)
                {
                    MessageError = "Error : Sub Category exists under " + Subcategoryexists.First().Category.Name + " category. Use another name";
                }
                else
                {
                    _db.SubCategory.Add(model.SubCategory);
                        await _db.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            SubCategory_Category_ViewModel model1 = new SubCategory_Category_ViewModel()
            {
                CategoryList = await _db.Category.ToListAsync(),
                SubCategory = model.SubCategory,
                SubCategoryList = await _db.SubCategory.OrderBy(x => x.Name).Select(x => x.Name).ToListAsync(),
                Message = MessageError

            };
               return View(model1);
        }


        [ActionName("GetSubCategory")]
        public async Task<IActionResult> GetSubCategory(int id)
        {
            List<SubCategory> subcategories = new List<SubCategory>();

            subcategories = await (from subCategory in _db.SubCategory
                             where subCategory.CategoryId == id
                             select subCategory).ToListAsync();
            return Json(new SelectList(subcategories, "Id", "Name"));
        }


        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var SubCategory = await _db.SubCategory.SingleOrDefaultAsync(x => x.Id == id);

            if(SubCategory == null)
            {
                return NotFound();
            }

            SubCategory_Category_ViewModel model = new SubCategory_Category_ViewModel()
            {
                CategoryList = await _db.Category.ToListAsync(),
                SubCategory = SubCategory,
                SubCategoryList = await _db.SubCategory.OrderBy(x => x.Name).Select(x => x.Name).Distinct().ToListAsync()
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,SubCategory_Category_ViewModel model)
        {
            if (ModelState.IsValid)
            {
                var Subcategoryexists = _db.SubCategory.Include(x => x.Category).Where(x => x.Name == model.SubCategory.Name && x.Category.Id == model.SubCategory.CategoryId);

                if (Subcategoryexists.Count() > 0)
                {
                    MessageError = "Error : Sub Category exists under " + Subcategoryexists.First().Category.Name + " category. Use another name";
                }
                else
                {
                    var SubCategoryDb = await _db.SubCategory.FindAsync(model.SubCategory.Id);
                    SubCategoryDb.Name = model.SubCategory.Name;

                    await _db.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            SubCategory_Category_ViewModel model1 = new SubCategory_Category_ViewModel()
            {
                CategoryList = await _db.Category.ToListAsync(),
                SubCategory = model.SubCategory,
                SubCategoryList = await _db.SubCategory.OrderBy(x => x.Name).Select(x => x.Name).ToListAsync(),
                Message = MessageError

            };
            return View(model1);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var SubCategory = await _db.SubCategory.SingleOrDefaultAsync(x => x.Id == id);

            if (SubCategory == null)
            {
                return NotFound();
            }

            SubCategory_Category_ViewModel model = new SubCategory_Category_ViewModel()
            {
                CategoryList = await _db.Category.ToListAsync(),
                SubCategory = SubCategory,
                SubCategoryList = await _db.SubCategory.OrderBy(x => x.Name).Select(x => x.Name).Distinct().ToListAsync()
            };
            return View(model);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var SubCategory = await _db.SubCategory.SingleOrDefaultAsync(x => x.Id == id);

            if (SubCategory == null)
            {
                return NotFound();
            }

            SubCategory_Category_ViewModel model = new SubCategory_Category_ViewModel()
            {
                CategoryList = await _db.Category.ToListAsync(),
                SubCategory = SubCategory,
                SubCategoryList = await _db.SubCategory.OrderBy(x => x.Name).Select(x => x.Name).Distinct().ToListAsync()
            };
            return View(model);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAction(int? ID)
        {
            var Subcategory = await _db.SubCategory.FindAsync(ID);
            if (Subcategory == null)
            {
                return View();
            }
            _db.SubCategory.Remove(Subcategory);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));

        }
    }
}
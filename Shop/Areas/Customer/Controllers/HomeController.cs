using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shop.Data;
using Shop.Models;
using Shop.Models.ViewModels;
using Shop.Utility;

namespace Shop.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {

        private readonly ApplicationDbContext _db;

        public HomeController(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            IndexViewModel IndexVM = new IndexViewModel()
            {
                Product = await _db.Product.Include(x => x.Category).Include(x => x.SubCategory).ToListAsync(),
                Category = await _db.Category.ToListAsync(),
            };

            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            if (claim!=null)
            {
                var count = _db.ShoppingCart.Where(x => x.UserId == claim.Value).ToList().Count;
                HttpContext.Session.SetInt32(StaticDefault.ssShopingCartCount, count);
            }

            return View(IndexVM);
        }

        [Authorize]
        public async Task<IActionResult> Details(int id)
        {
            var ProductDb = await _db.Product.Include(x => x.Category).Include(x => x.SubCategory).Where(x => x.Id == id).FirstOrDefaultAsync();

            ShoppingCart Productobject=  new ShoppingCart()
            {
                Product=ProductDb,
                ProductId=ProductDb.Id
            };
            return View(Productobject);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ShoppingCart ProductObject)
        {
            ProductObject.Id = 0;
            if (ModelState.IsValid)
            {
                var claimsIdentity=(ClaimsIdentity)this.User.Identity;
                var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                ProductObject.UserId = claim.Value;

                ShoppingCart ProductObjectDb = await _db.ShoppingCart.Where(x => x.UserId == ProductObject.UserId 
                && x.ProductId == ProductObject.ProductId).FirstOrDefaultAsync();

                if (ProductObjectDb == null)
                {   
                    await _db.ShoppingCart.AddAsync(ProductObject);
                }
                else
                {
                    ProductObjectDb.Count = ProductObjectDb.Count + ProductObject.Count;
                }
                await _db.SaveChangesAsync();

                var count = _db.ShoppingCart.Where(x => x.UserId == ProductObject.UserId).ToList().Count();
                HttpContext.Session.SetInt32(StaticDefault.ssShopingCartCount, count);

                return RedirectToAction("Index");
            }
            else
            {
                var ProductDb = await _db.Product.Include(x => x.Category).Include(x => x.SubCategory).Where(x => x.Id == ProductObject.ProductId).FirstOrDefaultAsync();

                ShoppingCart Productobject = new ShoppingCart()
                {
                    Product = ProductDb,
                    ProductId = ProductDb.Id
                };
                return View(Productobject);
            }

        }

        [Authorize]
        public async Task<IActionResult> CreateProduct(int id)
        {
            var ProductDb = await _db.Product.Include(x => x.Category).Include(x => x.SubCategory).Where(x => x.Id == id).FirstOrDefaultAsync();

            ShoppingCart Productobject = new ShoppingCart()
            {
                Product = ProductDb,
                ProductId = ProductDb.Id
            };
            Productobject.Id = 0;

                var claimsIdentity = (ClaimsIdentity)this.User.Identity;
                var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            Productobject.UserId = claim.Value;

                ShoppingCart ProductObjectDb = await _db.ShoppingCart.Where(x => x.UserId == Productobject.UserId
                && x.ProductId == Productobject.ProductId).FirstOrDefaultAsync();

                if (ProductObjectDb == null)
                {
                    await _db.ShoppingCart.AddAsync(Productobject);
                }
                else
                {
                    ProductObjectDb.Count = ProductObjectDb.Count + Productobject.Count;
                }
                await _db.SaveChangesAsync();

                var count = _db.ShoppingCart.Where(x => x.UserId == Productobject.UserId).ToList().Count();
                HttpContext.Session.SetInt32(StaticDefault.ssShopingCartCount, count);
                   
                return RedirectToAction(nameof(Index));
            
        }




        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

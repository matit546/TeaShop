using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shop.Data;
using Shop.Models;
using Shop.Utility;

namespace Shop.Areas.Admin.Controllers
{
    [Authorize(Roles = StaticDefault.ManagerUser)]
    [Area("Admin")]
    public class ShippingMethodController : Controller
    {
        private readonly ApplicationDbContext _db;

        public ShippingMethodController(ApplicationDbContext db)
        {
            _db = db;
        }
        public async Task<IActionResult> Index()
        {
            return View(await _db.ShippingMethod.ToListAsync());
        }

        public IActionResult Create()
        {
            return View();
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ShippingMethod shippingMethod)
        {
            if (ModelState.IsValid)
            {
                _db.ShippingMethod.Add(shippingMethod);
                await _db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(shippingMethod);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _db.ShippingMethod.FindAsync(id);

            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ShippingMethod shippingMethod)
        {
            if (ModelState.IsValid)
            {
                _db.Update(shippingMethod);
                await _db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(shippingMethod);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var shippingMethod = await _db.ShippingMethod.FindAsync(id);

            if (shippingMethod == null)
            {
                return NotFound();
            }

            return View(shippingMethod);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAction(int? ID)
        {
            var shippingMethod = await _db.ShippingMethod.FindAsync(ID);
            if (shippingMethod == null)
            {
                return View();
            }
            _db.ShippingMethod.Remove(shippingMethod);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));

        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var shippingMethod = await _db.ShippingMethod.FindAsync(id);

            return View(shippingMethod);
        }
    }
}
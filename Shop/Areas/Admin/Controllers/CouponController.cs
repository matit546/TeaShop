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
    public class CouponController : Controller
    {
        private readonly ApplicationDbContext _db;

        public CouponController(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _db.Coupon.ToListAsync());
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Coupon Coupons)
        {
            if (ModelState.IsValid)
            {
                await _db.Coupon.AddAsync(Coupons);
                await _db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(Coupons);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null){
                return NotFound();
            }
            var Coupon = await _db.Coupon.FindAsync(id);
            if (Coupon == null)
            {
                return NotFound();
            }
            return View(Coupon);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Coupon coupon)
        {
            if (ModelState.IsValid)
            {
                var CouponDb = await _db.Coupon.FindAsync(coupon.Id);
                if (CouponDb == null)
                {
                    return NotFound();
                }
                CouponDb.Name = coupon.Name;
                CouponDb.CouponType = coupon.CouponType;
                CouponDb.Discount = coupon.Discount;
                CouponDb.IsActive = coupon.IsActive;
                CouponDb.MininumAmount = coupon.MininumAmount;

                await _db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(coupon);

        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var Coupon = await _db.Coupon.FindAsync(id);
            if (Coupon == null)
            {
                return NotFound();
            }
            return View(Coupon);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var Coupon = await _db.Coupon.FindAsync(id);
            if (Coupon == null)
            {
                return NotFound();
            }
            return View(Coupon);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAction(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var coupon = await _db.Coupon.FindAsync(id);
            if (coupon == null)
            {
                return NotFound();
            }
          
            _db.Coupon.Remove(coupon);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
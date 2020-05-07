using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Shop.Data;
using Shop.Models;
using Shop.Models.ViewModels;
using Shop.Utility;
using Stripe;

namespace Shop.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class CartController : Controller
    {

        private readonly ApplicationDbContext _db;

        private static bool IfPayment { get; set; }

        [BindProperty]
        public OrderDetailsCart orderDetailCart { get; set; }


        public CartController(ApplicationDbContext db)
        {
            _db = db;
        }


        public async Task<IActionResult> Index()
        {
            
            orderDetailCart = new OrderDetailsCart()
            {
                OrderHeader = new Models.OrderHeader(),
                ShippingMethod= await _db.ShippingMethod.ToListAsync()

           };
            orderDetailCart.OrderHeader.OrderTotal = 0;

            var claimsIdentity = (ClaimsIdentity)User.Identity; ;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            var cart = _db.ShoppingCart.Where(x => x.UserId == claim.Value);
            if (cart != null)
            {
                orderDetailCart.ListCart = cart.ToList();
            }

            foreach (var list in orderDetailCart.ListCart)
            {
                list.Product = await _db.Product.FirstOrDefaultAsync(x => x.Id == list.ProductId);
                orderDetailCart.OrderHeader.OrderTotal = orderDetailCart.OrderHeader.OrderTotal + (list.Product.Price * list.Count);
            }
            orderDetailCart.OrderHeader.OrderTotalOriginal = orderDetailCart.OrderHeader.OrderTotal;
            if (HttpContext.Session.GetString(StaticDefault.ssCouponCode) != null)
            {
                orderDetailCart.OrderHeader.CouponCode = HttpContext.Session.GetString(StaticDefault.ssCouponCode);
                var couponDb = await _db.Coupon.Where(x => x.Name.ToUpper() == orderDetailCart.OrderHeader.CouponCode.ToUpper()).FirstOrDefaultAsync();
                orderDetailCart.OrderHeader.OrderTotal = StaticDefault.DiscountedPrice(couponDb, orderDetailCart.OrderHeader.OrderTotalOriginal);
            }
            return View(orderDetailCart);
        }


        public async Task<IActionResult> Summary()
        {
            orderDetailCart = new OrderDetailsCart()
            {
                OrderHeader = new Models.OrderHeader(),
                ShippingMethod = await _db.ShippingMethod.ToListAsync()
            };
            orderDetailCart.OrderHeader.OrderTotal = 0;

            var claimsIdentity = (ClaimsIdentity)User.Identity; ;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            User user = await _db.User.Where(v => v.Id == claim.Value).FirstOrDefaultAsync();

            var cart = _db.ShoppingCart.Where(x => x.UserId == claim.Value);
            if (cart != null)
            {
                orderDetailCart.ListCart = cart.ToList();
            }

            foreach (var list in orderDetailCart.ListCart)
            {
                list.Product = await _db.Product.FirstOrDefaultAsync(x => x.Id == list.ProductId);
                orderDetailCart.OrderHeader.OrderTotal = orderDetailCart.OrderHeader.OrderTotal + (list.Product.Price * list.Count);
            }
   
            orderDetailCart.OrderHeader.OrderTotalOriginal = orderDetailCart.OrderHeader.OrderTotal;
            orderDetailCart.OrderHeader.PickupName = user.Name;
            orderDetailCart.OrderHeader.StreetAddress = user.StreetAddress;
            orderDetailCart.OrderHeader.PostalCode = user.PostalCode;
            orderDetailCart.OrderHeader.State = user.State;
            orderDetailCart.OrderHeader.City = user.City;
            orderDetailCart.OrderHeader.PhoneNumber = user.PhoneNumber;

            ViewData["ShippingMethodId"] = new SelectList(_db.ShippingMethod, "Id", "Name");
            if (HttpContext.Session.GetString(StaticDefault.ssCouponCode) != null)
            {
                orderDetailCart.OrderHeader.CouponCode = HttpContext.Session.GetString(StaticDefault.ssCouponCode);
                var couponDb = await _db.Coupon.Where(x => x.Name.ToUpper() == orderDetailCart.OrderHeader.CouponCode.ToUpper()).FirstOrDefaultAsync();
                orderDetailCart.OrderHeader.OrderTotal = StaticDefault.DiscountedPrice(couponDb, orderDetailCart.OrderHeader.OrderTotalOriginal);
            }

            return View(orderDetailCart);
        }




        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Summary")]
        public async Task<IActionResult> SummaryPost()
        {

            var claimsIdentity = (ClaimsIdentity)User.Identity; ;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            orderDetailCart.ListCart = await _db.ShoppingCart.Where(x => x.UserId == claim.Value).ToListAsync();

            orderDetailCart.OrderHeader.PaymentStatus = StaticDefault.PaymentStatusPending;
            orderDetailCart.OrderHeader.OrderDate = DateTime.Now;
            orderDetailCart.OrderHeader.Status = StaticDefault.PaymentStatusPending;
            orderDetailCart.OrderHeader.ShippingMethodId = Convert.ToInt32(Request.Form["ShippingMethodId"].ToString());
            orderDetailCart.OrderHeader.UserId = claim.Value;
            orderDetailCart.OrderHeader.Estimated_delivery = DateTime.Now.AddDays(7);
            List<OrderDetailsCart> orderdetailcart= new List<OrderDetailsCart>();
            _db.OrderHeader.Add(orderDetailCart.OrderHeader);

            orderDetailCart.OrderHeader.OrderTotalOriginal = 0;
            
            foreach (var item in orderDetailCart.ListCart)
            {
                item.Product = await _db.Product.FirstOrDefaultAsync(x => x.Id == item.ProductId);
                OrderDetail _orderdetail = new OrderDetail
                {
                    ProductId = item.ProductId,
                    OrderId = orderDetailCart.OrderHeader.Id,
                    Name=item.Product.Name,
                    Description = item.Product.Description,
                    Price = item.Product.Price,
                    Count = item.Count
                    
                };
                orderDetailCart.OrderHeader.OrderTotalOriginal += _orderdetail.Count * _orderdetail.Price;
                _db.OrderDetail.Add(_orderdetail);
            }

            var shippingcost = _db.ShippingMethod.Where(x => x.Id == orderDetailCart.OrderHeader.ShippingMethodId).FirstOrDefault();

         
            ViewData["ShippingMethodId"] = new SelectList(_db.ShippingMethod, "Id", "Name");
            if (HttpContext.Session.GetString(StaticDefault.ssCouponCode) != null)
            {
                orderDetailCart.OrderHeader.CouponCode = HttpContext.Session.GetString(StaticDefault.ssCouponCode);
                var couponDb = await _db.Coupon.Where(x => x.Name.ToUpper() == orderDetailCart.OrderHeader.CouponCode.ToUpper()).FirstOrDefaultAsync();
                orderDetailCart.OrderHeader.OrderTotal = StaticDefault.DiscountedPrice(couponDb, orderDetailCart.OrderHeader.OrderTotalOriginal);
                orderDetailCart.OrderHeader.CouponCodeDiscount = orderDetailCart.OrderHeader.OrderTotalOriginal - orderDetailCart.OrderHeader.OrderTotal;
                orderDetailCart.OrderHeader.OrderTotal = orderDetailCart.OrderHeader.OrderTotal + shippingcost.Price;
            }
            else
            {
                orderDetailCart.OrderHeader.OrderTotal = orderDetailCart.OrderHeader.OrderTotalOriginal;
                orderDetailCart.OrderHeader.CouponCodeDiscount = orderDetailCart.OrderHeader.OrderTotalOriginal - orderDetailCart.OrderHeader.OrderTotal;
                orderDetailCart.OrderHeader.OrderTotal = orderDetailCart.OrderHeader.OrderTotal + shippingcost.Price;
            }   
            if (orderDetailCart.OrderHeader.CouponCodeDiscount <= 0)
            {
                orderDetailCart.OrderHeader.CouponCodeDiscount = 0;
            }
            _db.ShoppingCart.RemoveRange(orderDetailCart.ListCart);
            HttpContext.Session.SetInt32(StaticDefault.ssShopingCartCount, 0);
           
            await _db.SaveChangesAsync();

            IfPayment = true;
            return RedirectToAction("Payment");
          
        }

        [Authorize]
        [HttpPost, ActionName("Pay")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PaymentPay(string stripeToken)
        {
            orderDetailCart = new OrderDetailsCart()
            {
                OrderHeader = new Models.OrderHeader(),
                ShippingMethod = await _db.ShippingMethod.ToListAsync()
            };
            var claimsIdentity = (ClaimsIdentity)User.Identity; ;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            var Transaction = await _db.OrderHeader.Where(x => x.UserId == claim.Value).LastOrDefaultAsync();

            var options = new ChargeCreateOptions
            {
                Amount = Convert.ToInt32(Transaction.OrderTotal * 100),
                Currency = "usd",
                Description = "Order Id" + Transaction.Id,
                Source = stripeToken
            };
            var service = new ChargeService();
            Charge charge = service.Create(options);

            if (charge.BalanceTransactionId == null)
            {
                Transaction.PaymentStatus = StaticDefault.PaymentStatusRejected;
            }
            else
            {
                Transaction.TransactionId = charge.BalanceTransactionId;
            }

            if (charge.Status.ToLower() == "succeeded")
            {
                Transaction.PaymentStatus = StaticDefault.PaymentStatusApproved;
                Transaction.Status = StaticDefault.StatusSubmitted;
            }
            else
            {
                Transaction.PaymentStatus = StaticDefault.PaymentStatusRejected;
            }

            await _db.SaveChangesAsync();
            return RedirectToAction("Confirm", "Order", new { id = Transaction.Id });
        }

        [Authorize]
        public IActionResult Payment()
        {
            if (IfPayment == false)
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                IfPayment = false;
                orderDetailCart = new OrderDetailsCart()
                {
                    OrderHeader = new Models.OrderHeader(),
                    ShippingMethod =  _db.ShippingMethod.ToList()
                };
                var claimsIdentity = (ClaimsIdentity)User.Identity; ;
                var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                var PaymentCost = _db.OrderHeader.Where(x => x.UserId == claim.Value).LastOrDefault();
                orderDetailCart.OrderHeader.OrderTotal = PaymentCost.OrderTotal;
                orderDetailCart.OrderHeader.OrderTotal *= 100;
                return View(orderDetailCart);
            }
        }

        [Authorize]
        [HttpPost, ActionName("PayNormal")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PaymentPayNormal(string stripeToken)
        {
            orderDetailCart = new OrderDetailsCart()
            {
                OrderHeader = new Models.OrderHeader(),
                ShippingMethod = await _db.ShippingMethod.ToListAsync()
            };
            var claimsIdentity = (ClaimsIdentity)User.Identity; ;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            var Transaction = await _db.OrderHeader.Where(x => x.UserId == claim.Value).LastOrDefaultAsync();

                Transaction.PaymentStatus = StaticDefault.PaymentStatusApproved;
                Transaction.Status = StaticDefault.StatusSubmitted;
            

            await _db.SaveChangesAsync();
            return RedirectToAction("Confirm", "Order", new { id = Transaction.Id });
        }

        public IActionResult AddCoupon()
        {
            if (orderDetailCart.OrderHeader.CouponCode == null)
            {
                orderDetailCart.OrderHeader.CouponCode = "";
            }
            HttpContext.Session.SetString(StaticDefault.ssCouponCode, orderDetailCart.OrderHeader.CouponCode);

            return RedirectToAction(nameof(Index));
        }

        public IActionResult RemoveCoupon()
        {
            HttpContext.Session.SetString(StaticDefault.ssCouponCode, string.Empty);
            return RedirectToAction(nameof(Index));
        }


        public async Task<IActionResult> Plus(int cartId)
        {
            var cart = await _db.ShoppingCart.FirstOrDefaultAsync(i => i.Id == cartId);
            cart.Count += 1;
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Minus(int cartId)
        {
            var cart = await _db.ShoppingCart.FirstOrDefaultAsync(i => i.Id == cartId);
            if (cart.Count == 1)
            {
                _db.ShoppingCart.Remove(cart);
                await _db.SaveChangesAsync();

                var cnt = _db.ShoppingCart.Where(x => x.UserId == cart.UserId).ToList().Count;
                HttpContext.Session.SetInt32(StaticDefault.ssShopingCartCount, cnt);
            }
            else
            {
                cart.Count -= 1;
                await _db.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Remove(int cartId)
        {
            var cart = await _db.ShoppingCart.FirstOrDefaultAsync(i => i.Id == cartId);

            _db.ShoppingCart.Remove(cart);
            await _db.SaveChangesAsync();

            var cnt = _db.ShoppingCart.Where(x => x.UserId == cart.UserId).ToList().Count;
            HttpContext.Session.SetInt32(StaticDefault.ssShopingCartCount, cnt);
            return RedirectToAction(nameof(Index));

        }

       

    }
}
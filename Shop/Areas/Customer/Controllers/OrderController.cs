using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Shop.Data;
using Shop.Models;
using Shop.Models.ViewModels;
using Shop.Utility;

namespace Shop.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class OrderController : Controller
    {

        private ApplicationDbContext _db;
        private readonly int PageSize = 5;
        public OrderController(ApplicationDbContext db)
        {
            _db = db;
        }

        [Authorize]
        public async Task<IActionResult> Confirm(int id)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            OrderDetailsViewModel orderDetailsViewModel = new OrderDetailsViewModel()
            {
                OrderHeader = await _db.OrderHeader.Include(x => x.ShippingMethod).Include(u => u.User).FirstOrDefaultAsync(u => u.Id == id && u.UserId == claim.Value),
                OrderDetails = await _db.OrderDetail.Where(u => u.OrderId == id).ToListAsync()
            };

            return View(orderDetailsViewModel);
        }

        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        public async Task<IActionResult> OrderHistory(int productPage = 1)
        {

            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            OrderListViewModel orderListViewModel = new OrderListViewModel()
            {
                Orders = new List<OrderDetailsViewModel>()
            };
            List<OrderHeader> orderHeadersList;
            if (User.IsInRole("Manager"))
            {
                 orderHeadersList = await _db.OrderHeader.ToListAsync();
            }
            else
            {
                orderHeadersList = await _db.OrderHeader.Include(o => o.User).Where(u => u.UserId == claim.Value).ToListAsync();
            }

            foreach (OrderHeader item in orderHeadersList)
            {
                OrderDetailsViewModel individual = new OrderDetailsViewModel()
                {
                    OrderHeader = item,
                    OrderDetails = await _db.OrderDetail.Where(x => x.OrderId == item.Id).ToListAsync()
                };
                orderListViewModel.Orders.Add(individual);

            }

            var count = orderListViewModel.Orders.Count;
            orderListViewModel.Orders = orderListViewModel.Orders.OrderByDescending(p => p.OrderHeader.Id)
                           .Skip((productPage - 1) * PageSize)
                           .Take(PageSize).ToList();

            orderListViewModel.PagingInfo = new PagingInfo()
            {
                CurrentPage = productPage,
                ItemsPerPage = PageSize,
                TotalItem = count,
                urlParam = "/Customer/Order/OrderHistory?productPage=:"
            };

            return View(orderListViewModel);

        }

        public async Task<IActionResult> GetOrderDetails(int id)
        {
            OrderDetailsViewModel orderDetailsViewModel = new OrderDetailsViewModel()
            {
                OrderHeader = await _db.OrderHeader.Include(x => x.ShippingMethod).FirstOrDefaultAsync(i => i.Id == id),
                OrderDetails = await _db.OrderDetail.Where(x => x.OrderId == id).ToListAsync()
            };
            orderDetailsViewModel.OrderHeader.User = await _db.User.FirstOrDefaultAsync(u => u.Id == orderDetailsViewModel.OrderHeader.UserId);
            return PartialView("_IndividualOrderDetailsView", orderDetailsViewModel);
        }


        [Authorize(Roles = StaticDefault.ManagerUser + "," + StaticDefault.ShopUser)]
        public async Task<IActionResult> ManageOrder()
        {

            List<OrderDetailsViewModel> orderListViewModels = new List<OrderDetailsViewModel>();

            List<OrderHeader> orderHeaders = await _db.OrderHeader.Include(s => s.ShippingMethod).Where(o => o.Status == StaticDefault.StatusSubmitted || o.Status == StaticDefault.StatusInProccess)
                .OrderByDescending(o => o.OrderDate).ToListAsync();

            foreach (OrderHeader item in orderHeaders)
            {
                OrderDetailsViewModel individual = new OrderDetailsViewModel()
                {
                    OrderHeader = item,
                    OrderDetails = await _db.OrderDetail.Where(x => x.OrderId == item.Id).ToListAsync()
                };
                orderListViewModels.Add(individual);

            }

            return View(orderListViewModels.OrderBy(o => o.OrderHeader.OrderDate).ToList());

        }

        [Authorize(Roles = StaticDefault.ShopUser + "," + StaticDefault.ManagerUser)]
        public async Task<IActionResult> OrderSubmit(int OrderId)
        {
            OrderHeader orderHeader = await _db.OrderHeader.FindAsync(OrderId);
            orderHeader.Status = StaticDefault.StatusInProccess;
            await _db.SaveChangesAsync();
            return RedirectToAction("ManageOrder", "Order");

        }

        [Authorize(Roles = StaticDefault.ShopUser + "," + StaticDefault.ManagerUser)]
        public async Task<IActionResult> OrderReady(int OrderId)
        {
            OrderHeader orderHeader = await _db.OrderHeader.Include(s => s.ShippingMethod).Where(o => o.Id == OrderId).FirstAsync();
            if (orderHeader.ShippingMethod.Name == StaticDefault.LocalPickup)
            {
                orderHeader.Status = StaticDefault.StatusReady;
            }
            else
            {
                orderHeader.Status = StaticDefault.StatusReadyForShihment;
            }
            await _db.SaveChangesAsync();

            //Email
            return RedirectToAction("ManageOrder", "Order");

        }

        [Authorize(Roles = StaticDefault.ShopUser + "," + StaticDefault.ManagerUser)]
        public async Task<IActionResult> OrderCancel(int OrderId)
        {
            OrderHeader orderHeader = await _db.OrderHeader.FindAsync(OrderId);

            orderHeader.Status = StaticDefault.StatusCancelled;
            await _db.SaveChangesAsync();

            //Email
            return RedirectToAction("ManageOrder", "Order");

        }

        [Authorize]
        public async Task<IActionResult> OrderPickup(int productPage = 1, string searchEmail = null, string searchName = null, string searchPhone = null)
        {

            //var claimsIdentity = (ClaimsIdentity)User.Identity;
            //var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            OrderListViewModel orderListViewModel = new OrderListViewModel()
            {
                Orders = new List<OrderDetailsViewModel>()
            };

            StringBuilder param = new StringBuilder();
            param.Append("/Customer/Order/OrderPickup?productPage=:");
            param.Append("&searchName=");
            if (searchName != null)
            {
                param.Append(searchName);
            }
            if (searchEmail != null)
            {
                param.Append(searchEmail);
            }
            if (searchPhone != null)
            {
                param.Append(searchPhone);
            }
            List<OrderHeader> orderHeadersList = new List<OrderHeader>();
            if (searchName != null || searchEmail != null || searchPhone != null)
            {

                var user = new User();
                if (searchName != null)
                {
                    orderHeadersList = await _db.OrderHeader.Include(o => o.User)
                        .Where(p => p.PickupName.ToLower().Contains(searchName.ToLower()))
                        .OrderByDescending(o => o.OrderDate).ToListAsync();
                }
                else
                {
                    if (searchEmail != null)
                    {
                        user = await _db.User.Where(u => u.Email.ToLower().Contains(searchEmail.ToLower())).FirstOrDefaultAsync();
                        orderHeadersList = await _db.OrderHeader.Include(o => o.User)
                            .Where(u => u.UserId == user.Id)
                            .OrderByDescending(o => o.OrderDate).ToListAsync();
                    }
                    else
                    {
                        if (searchPhone != null)
                        {
                            orderHeadersList = await _db.OrderHeader.Include(o => o.User)
                           .Where(p => p.PhoneNumber.Contains(searchPhone))
                           .OrderByDescending(o => o.OrderDate).ToListAsync();
                        }
                    }
                }

            }

            else
            {
                orderHeadersList = await _db.OrderHeader.Include(o => o.User).Where(u => u.Status == StaticDefault.StatusReady || u.Status == StaticDefault.StatusReadyForShihment).ToListAsync();
            }

                foreach (OrderHeader item in orderHeadersList)
                {
                    OrderDetailsViewModel individual = new OrderDetailsViewModel()
                    {
                        OrderHeader = item,
                        OrderDetails = await _db.OrderDetail.Where(x => x.OrderId == item.Id).ToListAsync()
                    };
                    orderListViewModel.Orders.Add(individual);

                }
            

            var count = orderListViewModel.Orders.Count;
            orderListViewModel.Orders = orderListViewModel.Orders.OrderByDescending(p => p.OrderHeader.Id)
                           .Skip((productPage - 1) * PageSize)
                           .Take(PageSize).ToList();

            orderListViewModel.PagingInfo = new PagingInfo()
            {
                CurrentPage = productPage,
                ItemsPerPage = PageSize,
                TotalItem = count,
                urlParam = param.ToString()
            };

            return View(orderListViewModel);

        }

        [Authorize(Roles=StaticDefault.FrontDeskUser+","+StaticDefault.ManagerUser)]
        public async Task<IActionResult> OrderDone(int OrderId)
        {
            OrderHeader orderHeader = await _db.OrderHeader.FindAsync(OrderId);
            orderHeader.Status = StaticDefault.StatusCompleted;
            await _db.SaveChangesAsync();
            return RedirectToAction("OrderPickup", "Order");
        }


        [Authorize(Roles = StaticDefault.FrontDeskUser + "," + StaticDefault.ManagerUser)]
        public async Task<IActionResult> OrderShipped(int OrderId)
        {
            OrderHeader orderHeader = await _db.OrderHeader.FindAsync(OrderId);
            orderHeader.Status = StaticDefault.StatusShipped;
            await _db.SaveChangesAsync();
            return RedirectToAction("OrderPickup", "Order");
        }

    }
}
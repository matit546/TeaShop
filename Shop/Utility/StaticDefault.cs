using Shop.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shop.Utility
{
    public class StaticDefault
    {
        public const string DefaultProductImage = "default.jpg";

        public const string ManagerUser = "Manager";
        public const string ShopUser = "Shop";
        public const string FrontDeskUser = "FrontDesk";
        public const string CustomerUser = "Customer";
            
        public const string StatusSubmitted = "Submitted";
        public const string StatusInProccess = "Submitted for implementation";
        public const string StatusReady = "Ready";
        public const string StatusCompleted = "Completed";
        public const string LocalPickup = "Pick up at the local store $0";
        public const string StatusReadyForShihment = "The order is ready for shipment";
        public const string StatusShipped = "Has been Shipped";
        public const string StatusCancelled = "Cancelled";

        public const string PaymentStatusPending = "Pending";
        public const string PaymentStatusApproved = "Approved";
        public const string PaymentStatusRejected = "Rejected";

        public const string Weight100 = "100g";
        public const string Weight250 = "250g";
        public const string Weight500 = "500g";
        public const string Weight1000 = "1000g";

        public const string ssShopingCartCount = "ssCartCount";
        public const string ssCouponCode = "ssCouponCode";




        public static double DiscountedPrice(Coupon couponDb, double OriginalOrderTotal)
        {
            if(couponDb==null)
            {
                return OriginalOrderTotal;
            }
            else
            {
                if(couponDb.MininumAmount> OriginalOrderTotal)
                {
                    return OriginalOrderTotal;
                }
                else
                {
                    if (Convert.ToInt32(couponDb.CouponType) == (int)Coupon.EcouponType.Dollar)
                    {
                        return Math.Round(OriginalOrderTotal - couponDb.Discount, 2);
                    }
                    else
                    {
                        if (Convert.ToInt32(couponDb.CouponType) == (int)Coupon.EcouponType.percent)
                        {
                            return Math.Round(OriginalOrderTotal - (OriginalOrderTotal*couponDb.Discount/100), 2);
                        }
                    }
                }
            }
            return OriginalOrderTotal;
        }
    }
}

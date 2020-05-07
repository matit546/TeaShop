using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Shop.Models
{
    public class OrderHeader
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        [Required]
        public DateTime OrderDate { get; set; }

        [Required]
        public double OrderTotalOriginal { get; set; }

        [Required]
        [DisplayFormat(DataFormatString = "{0:C}")]
        [Display(Name = "Order total")]
        public double OrderTotal { get; set; }

        [Display(Name = "Estimated delivery:")]
        public DateTime Estimated_delivery { get; set; }

        public string CouponCode { get; set; }
        public double CouponCodeDiscount { get; set; }

        public string Status { get; set; }
        public string PaymentStatus { get; set; }
        public string Comments { get; set; }

        [Display(Name = "Phone Number:")]
        public string PhoneNumber { get; set; }
            
        [Display(Name = "Pickup Name:")]
        public string PickupName { get; set; }

        public string TransactionId { get; set; }


        [Required]
        public int ShippingMethodId { get; set; }

        [ForeignKey("ShippingMethodId")]
        public virtual ShippingMethod ShippingMethod { get; set; }

        [Required]
        public string StreetAddress { get; set; }
        [Required]
        public string City { get; set; }
        [Required]
        public string State { get; set; }
        [Required]
        public string PostalCode { get; set; }

    }
}

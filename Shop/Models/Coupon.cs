﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Shop.Models
{
    public class Coupon
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string CouponType { get; set; }

        public enum EcouponType { percent = 0, Dollar = 1 }

        [Required]
        public double Discount { get; set; }

        [Required]
        public double MininumAmount { get; set; }

        public bool IsActive{ get; set; }
    }
}
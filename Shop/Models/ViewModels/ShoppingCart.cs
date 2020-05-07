using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Shop.Models.ViewModels
{
    public class ShoppingCart
    {
        public ShoppingCart()
        {
            Count = 1;
        }
        [Key]
        public int Id { get; set; }
        public string UserId { get; set; }
        public int ProductId { get; set; }

        [NotMapped]
        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        [NotMapped]
        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; }

        [Range(1, int.MaxValue, ErrorMessage ="Please enter a value greater or equal to{1}")]
        public int Count { get; set; }

    }
}

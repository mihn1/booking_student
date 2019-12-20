using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace B2S.Core.Entities
{
    public class RoomType : BaseEntity
    {
        public RoomType()
        {
            this.CreatedAt = DateTime.UtcNow;
        }

        [StringLength(38)]
        [Key]
        [Display(Name = "Room Type Id")]
        public string RoomTypeId { get; set; }
       
        [StringLength(25)]
        [Display(Name = "Name")]
        [Required]
        public string RoomTypeName { get; set; }
       
        [StringLength(5)]
        [Display(Name = "Value")]
        [Required]
        public string RoomTypeValue { get; set; }

        [StringLength(38)]
        [Display(Name = "Property Id")]
        [Required]
        public string PropertyId { get; set; }
        [Display(Name = "Property")]
        public Property Property { get; set; }

        [Display(Name = "Price")]
        public decimal Price { get; set; }

        [Display(Name = "Discount Amount")]
        public decimal DiscountAmount { get; set; }

        [Display(Name = "Deposit Amount")]
        public decimal DepositAmount { get; set; }

    }
}

using B2S.Core.Enums;
using B2S.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace B2S.Core.Entities
{
    public class Expense : BaseEntity
    {
        public Expense()
        {
            this.CreatedAt = DateTime.UtcNow;
        }

        [StringLength(38)]
        [Key]
        [Display(Name = "Expense Id")]
        public string ExpenseId { get; set; }      

        [Display(Name = "Description")]
        public string Description { get; set; }

        [StringLength(38)]
        [Display(Name = "Category")]
        public string CategoryId { get; set; }
        [Display(Name = "Category")]
        public ExpenseCategory Category { get; set; }

        [StringLength(38)]
        [Display(Name = "Reference#")]
        public string Reference { get; set; }

        [Display(Name = "Date")]
        public DateTime ExpenseDate { get; set; }

        [Display(Name = "Amount")]
        public decimal Amount { get; set; }

        [Display(Name = "Receipt")]
        public byte[] Receipt { get; set; }
        [StringLength(250)]
        [Display(Name = "File Name")]
        public string FileName { get; set; }

        [StringLength(38)]
        [Display(Name = "Vendor")]
        [Required]
        public string VendorId { get; set; }
        [Display(Name = "Vendor")]
        public Vendor Vendor { get; set; }

    }
}

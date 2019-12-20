using System;
using System.ComponentModel.DataAnnotations;

namespace B2S.Core.Entities
{
    public class ExpenseCategory : BaseEntity
    {
        public ExpenseCategory()
        {
            this.CreatedAt = DateTime.UtcNow;
        }

        [StringLength(38)]
        [Key]
        [Display(Name = "Category Id")]
        public string CategoryId { get; set; }

        [StringLength(250)]
        [Display(Name = "Name")]
        [Required]
        public string CategoryName { get; set; }

        [Display(Name = "Description")]
        public string Description { get; set; }

        [StringLength(25)]
        [Display(Name = "Account Code")]
        public string AccountCode { get; set; }

        [StringLength(10)]
        [Display(Name = "Color Hex")]
        public string ColorHex { get; set; }
    }
}

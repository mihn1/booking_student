using B2S.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace B2S.Core.Entities
{
    public class InvoiceItem : BaseEntity
    {
        public InvoiceItem()
        {
            this.CreatedAt = DateTime.UtcNow;
        }

        [StringLength(38)]
        [Key]
        [Display(Name = "Invoice Item Id")]
        public string InvoiceItemId { get; set; }

        [StringLength(38)]
        [Display(Name = "Invoice Id")]
        public string InvoiceId { get; set; }

        [StringLength(250)]
        [Display(Name = "Item")]
        public string Item { get; set; }

        [Display(Name = "Description")]
        public string Description { get; set; }

        [Display(Name = "Price")]
        public decimal Price { get; set; }

        [Display(Name = "Quantity")]
        public decimal Quantity { get; set; }

        [Display(Name = "Amount")]
        public decimal Amount { get; set; }
    }
}

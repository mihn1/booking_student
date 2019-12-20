using B2S.Core.Enums;
using B2S.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace B2S.Core.Entities
{
    public class Invoice : BaseEntity
    {
        public Invoice()
        {
            this.CreatedAt = DateTime.UtcNow;
        }

        [StringLength(38)]
        [Key]
        [Display(Name = "Invoice Id")]
        public string InvoiceId { get; set; }

        [StringLength(38)]
        [Display(Name = "Booking Id")]
        public string BookingId { get; set; }
        [Display(Name = "Booking")]
        public Booking Booking{ get; set; }

        [StringLength(38)]
        [Display(Name = "Invoice Ref#")]
        public string InvoiceNumber { get; set; }

        [Display(Name = "Invoice Date")]
        public DateTime InvoiceDate { get; set; }

        [Display(Name = "Due Date")]
        public DateTime DueDate { get; set; }

        [Display(Name = "Tax Amount")]
        public decimal TaxAmount { get; set; }

        [Display(Name = "Discount Amount")]
        public decimal DiscountAmount { get; set; }

        [Display(Name = "SubTotal")]
        public decimal SubTotal { get; set; }

        [Display(Name = "Invoice Amount")]
        public decimal InvoiceAmount { get; set; }

        [Display(Name = "Payment Status")]
        public PaymentStatus PaymentStatus { get; set; }

        [StringLength(38)]
        [Display(Name = "Agent Id")]
        public string AgentId { get; set; }
        [Display(Name = "Agent")]
        public Agent Agent { get; set; }

        [StringLength(38)]
        [Display(Name = "Customer Id")]
        public string CustomerId { get; set; }
        [Display(Name = "Customer")]
        public Customer Customer { get; set; }

        [Display(Name = "Invoice Items")]
        public List<InvoiceItem> InvoiceItems { get; set; } = new List<InvoiceItem>();
    }
}

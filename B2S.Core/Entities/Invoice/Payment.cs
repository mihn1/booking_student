using B2S.Core.Enums;
using B2S.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace B2S.Core.Entities
{
    public class Payment : BaseEntity
    {
        public Payment()
        {
            this.CreatedAt = DateTime.UtcNow;
        }

        [StringLength(38)]
        [Key]
        [Display(Name = "Payment Id")]
        public string PaymentId { get; set; }

        [StringLength(38)]
        [Display(Name = "Booking Id")]
        public string BookingId { get; set; }
        [Display(Name = "Booking")]
        public Booking Booking { get; set; }

        [StringLength(250)]
        [Display(Name = "Payee")]
        public string Payee { get; set; }

        [Display(Name = "Description")]
        public string Description { get; set; }

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

        [Display(Name = "Price")]
        public decimal Price { get; set; }

        [Display(Name = "Quantity")]
        public decimal Quantity { get; set; }

        [Display(Name = "Amount")]
        public decimal Amount { get; set; }

        [Display(Name = "Payment Date")]
        public DateTime PaymentDate { get; set; }

        [Display(Name = "Payment Method")]
        public PaymentMethod PaymentMethod { get; set; }

        [Display(Name = "Payment Status")]
        public PaymentStatus PaymentStatus { get; set; }

        [Display(Name = "Has Invoice")]
        public bool IsCreateInvoice { get; set; }
    }
}

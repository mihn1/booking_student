using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace B2S.Web.ViewModels.Invoice
{
    public class SendInvoiceViewModel
    {
        [Required]
        [StringLength(38)]
        public string InvoiceId { get; set; }

        [StringLength(38)]
        [Display(Name = "Invoice Ref")]
        public string InvoiceNumber { get; set; }

        [Display(Name = "Send To")]
        [EmailAddress]
        [Required]
        [StringLength(50)]
        public string SendToEmail { get; set; }

        [Display(Name = "Subject")]
        [StringLength(250)]
        public string Subject { get; set; }

        [Display(Name = "Body Text")]
        public string BodyHTML { get; set; }
    }
}

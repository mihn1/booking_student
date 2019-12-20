using B2S.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace B2S.Core.Entities
{
    public class Agent : BaseEntity, IBaseAddress
    {
        public Agent()
        {
            this.CreatedAt = DateTime.UtcNow;
        }

        [StringLength(38)]
        [Key]
        [Display(Name = "Agent Id")]
        public string AgentId { get; set; }

        [StringLength(250)]
        [Display(Name = "Business Name")]
        public string BusinessName { get; set; }

        [StringLength(250)]
        [Display(Name = "Legal Name")]
        public string LegalName { get; set; }

        [StringLength(25)]
        [Display(Name = "Business Number")]
        public string BusinessNumber { get; set; }

        [Display(Name = "Contact Name")]
        [StringLength(50)]
        public string ContactName { get; set; }

        [Display(Name = "Phone")]
        [StringLength(20)]
        public string Phone { get; set; }

        [Display(Name = "Fax")]
        [StringLength(20)]
        public string Fax { get; set; }

        [Display(Name = "Email")]
        [StringLength(50)]
        public string Email { get; set; }

        [Display(Name = "Address")]
        [StringLength(250)]
        public string Address { get; set; }

        [Display(Name = "City")]
        [StringLength(30)]
        public string City { get; set; }

        [Display(Name = "Province")]
        [StringLength(30)]
        public string Province { get; set; }

        [Display(Name = "Postcode")]
        [StringLength(10)]
        public string Postcode { get; set; }

        [Display(Name = "Country")]
        [StringLength(30)]
        public string Country { get; set; }

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; }

        [StringLength(38)]
        [Display(Name = "Invoice Setting")]
        public string InvoiceSettingId { get; set; }
        [Display(Name = "Invoice Setting")]
        public InvoiceSetting InvoiceSetting { get; set; }
    }
}

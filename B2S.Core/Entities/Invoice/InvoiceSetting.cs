using B2S.Core.Enums;
using B2S.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace B2S.Core.Entities
{
    public class InvoiceSetting : BaseEntity
    {
        public InvoiceSetting()
        {
            this.CreatedAt = DateTime.UtcNow;
        }

        [StringLength(38)]
        [Key]
        [Display(Name = "Invoice Setting Id")]
        public string InvoiceSettingId { get; set; }

        [StringLength(250)]
        [Display(Name = "Name")]
        public string Name { get; set; }

        [Display(Name = "Description")]
        public string Description { get; set; }

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; }

        //Business Info
        [StringLength(250)]
        [Display(Name = "Business Name")]
        public string BusinessName { get; set; }

        [StringLength(25)]
        [Display(Name = "Business Number")]
        public string BusinessNumber { get; set; }
        public string BusinessLogoUrl { get; set; } = "/images/logo.png";

        [Display(Name = "Contact Name")]
        [StringLength(50)]
        public string ContactName { get; set; }

        [Display(Name = "Phone")]
        [StringLength(20)]
        public string Phone { get; set; }

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

        [Display(Name = "Website")]
        [StringLength(50)]
        public string Website { get; set; }

        // Payment Settings
        [Display(Name = "Payment Term")]
        public int PaymentTerm { get; set; }


        [Display(Name = "Payment Method")]
        public PaymentMethod PaymentMethod { get; set; }

        [Display(Name = "Payment Note")]
        public string PaymentNote { get; set; }

        [Display(Name = "Currency")]
        [StringLength(5)]
        public string Currency { get; set; }

        [Display(Name = "Generate Invoice Every")]
        public int RecurringPeriod { get; set; }

        [Display(Name = "Recurring Type")]
        public RecurringInvoiceType RecurringType { get; set; }

        [Display(Name = "Auto Send Invoice")]
        public bool AutoSend { get; set; }

        [Display(Name = "Current Invoice #")]
        [StringLength(50)]
        public string CurrentInvoiceNo { get; set; }

        [Display(Name = "Prefix")]
        [StringLength(10)]
        public string PrefixInvoiceNo { get; set; }

        // Bank Account
        [StringLength(250)]
        [Display(Name = "Bank Name")]
        [Required]
        public string BankName { get; set; }

        [StringLength(250)]
        [Display(Name = "Account Name")]
        [Required]
        public string AccountName { get; set; }

        [StringLength(10)]
        [Display(Name = "BSB Number")]
        [Required]
        public string BSBNumber { get; set; }

        [StringLength(15)]
        [Display(Name = "Account Number")]
        [Required]
        public string AccountNumber { get; set; }

        // Tax Setting
        [StringLength(50)]
        [Display(Name = "Tax Code")]
        [Required]
        public string TaxCode { get; set; }

        [Display(Name = "Tax Percentage (%)")]
        public decimal TaxPercentage { get; set; }

        // Email Setting
        [StringLength(38)]
        [Display(Name = "Email Template")]
        public string EmailTemplateId { get; set; }
        [Display(Name = "Email Template")]
        public EmailTemplate EmailTemplate { get; set; }

    }
}

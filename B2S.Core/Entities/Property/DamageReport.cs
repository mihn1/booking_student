using B2S.Core.Enums;
using B2S.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace B2S.Core.Entities
{
    public class DamageReport : BaseEntity
    {
        public DamageReport()
        {
            this.CreatedAt = DateTime.UtcNow;
        }

        [StringLength(38)]
        [Key]
        [Display(Name = "Report Id")]
        public string ReportId { get; set; }

        [StringLength(250)]
        [Display(Name = "Report Name")]
        public string ReportName { get; set; }

        [Display(Name = "Type of Damage")]
        public DamageType DamageType { get; set; }

        [Display(Name = "Status")]
        public DamageStatus DamageStatus { get; set; }

        //[StringLength(38)]
        //[Display(Name = "Vendor")]
        //[Required]
        //public string VendorId { get; set; }
        //[Display(Name = "Vendor")]
        //public Vendor Vendor { get; set; }

        [StringLength(38)]
        [Display(Name = "Property")]
        [Required]
        public string PropertyId { get; set; }
        [Display(Name = "Property")]
        public Property Property { get; set; }

        [StringLength(38)]
        [Display(Name = "Booking Id")]
        public string BookingId { get; set; }

        [Display(Name = "Date of Incident")]
        public DateTime IncidentDate { get; set; }

        [Display(Name = "Estimate Cost Repair")]
        public decimal EstimateRepairCost { get; set; }

        [Display(Name = "Actual Repair Cost")]
        public decimal ActualRepairCost { get; set; }

        [Display(Name = "Description of the damage")]
        public string Description { get; set; }

        [Display(Name = "Customer Name")]
        [StringLength(50)]
        public string CustomerName { get; set; }

        [Display(Name = "Person Taking Report")]
        [StringLength(50)]
        public string ReportPerson { get; set; }

        [Display(Name = "Comments")]
        public string Comments { get; set; }


    }
}
